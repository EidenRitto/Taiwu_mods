using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Harmony12;
using UnityEngine;
using UnityEngine.UI;

namespace CharmsForGongFa
{
    struct Index
    {
        //public static Dictionary<int, string> PicIndex = new Dictionary<int, string>();

        public static Dictionary<int, int>
            TurnEvenIndex = new Dictionary<int, int>(),
            EventIndex = new Dictionary<int, int>(),
            GongFaIndex = new Dictionary<int, int>();

        public static Dictionary<int, List<int>>
            GongFaPowerIndex = new Dictionary<int, List<int>>();

    }

    /// <summary>
    /// ���ڶ�ȡ������ xxxDate.txt / ͼƬ��Դ
    /// </summary>
    public static class DateLoader
    {
        /// <summary>
        /// ��� NewDate ���Ƿ�ʧ OldDate �������� �ֵ��Keys�� ������NewDate��ʽ����ȷ��
        /// </summary>
        /// <param name="datename"></param>
        /// <param name="NewDate"></param>
        /// <param name="OldDate"></param>
        /// <returns></returns>
        public static bool IndexCheck(string datename, Dictionary<int, Dictionary<int, string>> NewDate,
            Dictionary<int, Dictionary<int, string>> OldDate)
        {
            if (NewDate.Keys.Count == 0 || OldDate.Keys.Count == 0)
            {
                Main.Logger.Log("�����ļ���δ��ȡ��������������");
                return false;
            }

            List<int> OK = new List<int>(OldDate.FirstOrDefault().Value.Keys);
            List<int> NK = new List<int>(NewDate.FirstOrDefault().Value.Keys);
            string[] error = OK.Except(NK).Select(n => Convert.ToString(n)).ToArray();
            if (error.Length == 0) return true;
            var text = string.Join(",", error);
            Main.Logger.Log("����" + datename + "��Ŀ¼����ȷ������Ŀ¼ȱʧ��" + text);

            return false;
        }   // End of IndexCheck

        /// <summary>
        /// ��ȡ������limit�����Keyֵ
        /// </summary>
        /// <param name="DateList"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public static int GetMaxid(Dictionary<int, Dictionary<int, string>> DateList, int limit = -1)
        {
            int Maxid = 0;
            List<int> Eventids_EX = new List<int>(DateList.Keys);
            if (Eventids_EX.Count > 0)
            {
                if (limit < 0)
                {
                    Maxid = Eventids_EX.Max();
                }
                else
                {
                    Maxid = Eventids_EX.Select(t => t < limit ? t : 0).Max();
                    //foreach (int id in Eventids_EX)
                    //{
                    //    if (id > Maxid && id <= limit) Maxid = id;
                    //}
                }
            }
            return Maxid;
        }   // End of GetMaxid

        /// <summary>
        /// ����EventDate
        /// </summary>
        public static Dictionary<int, int> LoadEventDate(string root, string txtname)
        {
            string path = Path.Combine(root, txtname);
            string text;
            Dictionary<int, int> IdRemap = new Dictionary<int, int>();     // ��Event_Date.txt�е��¼�id������뵽�б�ĩβ
            if (DateFile.instance.eventDate == null) return IdRemap;
            if (File.Exists(path))
            {
                text = File.OpenText(path).ReadToEnd();
            }
            else
            {
                Main.Logger.Log($"�����¼���Դʧ�ܣ�{path} ������");
                return IdRemap;
            }

            int count = 0;
            int count2 = 0;
            // ��Event_Date.txt���л��֣�ÿ��Ϊһ���¼�
            string[] EvevtString = text.Replace("\r", "").Split("\n"[0]);
            // ��ȡ�����¼���id
            string[] EventIDs = EvevtString[0].Split(',');
            // ��ǰ�����¼���id�ϼ�
            List<int> Eventids_EX = new List<int>(DateFile.instance.eventDate.Keys);
            int Maxid = Eventids_EX.Max();

            for (int i = 1; i < EvevtString.Length; i++)
            {
                int id;
                if (int.TryParse(EvevtString[i].Split(',')[0], out id))
                    IdRemap.Add(id, Maxid + id);
            }

            Dictionary<int, Dictionary<int, string>> NewEvents = new Dictionary<int, Dictionary<int, string>>();
            for (int i = 1; i < EvevtString.Length; i++)
            {
                // ���ÿ���¼����ַ���
                string[] EventBody = EvevtString[i].Split(',');
                var EventID = EventBody[0];
                if (EventID != "#" && EventID != "")
                {
                    // ������¼���ѡ�
                    string[] EventBranchs = EventBody[6].Split('|');
                    // �������¼���ѡ�
                    if (EventBranchs[0] != "")
                    {
                        // �滻id
                        for (int m = 0; m < EventBranchs.Length; m++)
                        {
                            int origin_id = int.Parse(EventBranchs[m]);
                            if (IdRemap.ContainsKey(origin_id))
                            {
                                EventBranchs[m] = IdRemap[origin_id].ToString();
                            }
                        }

                        // �ϲ��滻�õ����¼��б�д��ԭ�¼���
                        EventBody[6] = string.Join("|", EventBranchs);
                    }

                    // ͬ������ת�¼�
                    if (EventBody[8] != "" && EventBody[8] != "-1")
                    {
                        int origin_id = int.Parse(EventBody[8]);
                        if (IdRemap.ContainsKey(origin_id))
                        {
                            EventBody[8] = IdRemap[origin_id].ToString();
                        }
                    }

                    int id = int.Parse(EventID);
                    Dictionary<int, string> EventBodyDic = new Dictionary<int, string>();
                    for (int j = 0; j < EventIDs.Length; j++)
                    {
                        //#-ID��0-��ע
                        if (EventIDs[j] != "#" && EventIDs[j] != "" && int.Parse(EventIDs[j]) != 0)
                        {
                            EventBodyDic.Add(int.Parse(EventIDs[j]), Regex.Unescape(EventBody[j]));
                        }
                    }

                    NewEvents.Add((id + Maxid), EventBodyDic);
                    // ��Ч�¼�++
                    count++;
                }
            }

            if (!IndexCheck("eventDate", NewEvents, DateFile.instance.eventDate))
            {
                Main.Logger.Log("�����¼�ʧ��");
                return null;
            }

            lock (DateFile.instance.eventDate)
            {
                foreach (int id in NewEvents.Keys)
                {
                    if (!DateFile.instance.eventDate.Keys.Contains(id))
                    {
                        DateFile.instance.eventDate.Add(id, NewEvents[id]);
                        count2++;
                    }
                    else Main.Logger.Log("���� ID�ظ�:" + (id - Maxid));
                }
            }

            Main.Logger.Log("�ҵ���" + count + "���¼����ɹ�����" + count2 + "���¼���");
            return IdRemap;
        }   // End of LoadEventDate


        /// <summary>
        /// �Ե�һ��ΪKeyֵ����ȡDate�ļ�
        /// </summary>
        /// <param name="root"></param>
        /// <param name="txtname"></param>
        /// <param name="DateList"></param>
        /// <param name="passDateIndex"></param>
        /// <returns></returns>
        public static bool LoadBaseDate(string root, string txtname,
            out Dictionary<int, Dictionary<int, string>> DateList, int passDateIndex = -1)
        {
            DateList = new Dictionary<int, Dictionary<int, string>>();
            if (GetSprites.instance == null)
                return false;
            string path = Path.Combine(root, txtname);
            if (!File.Exists(path))
            {
                Main.Logger.Log($"{path} ������");
                return false;
            }

            // ��ȡtextColor �� Key List
            FieldInfo textColor0 = typeof(GetSprites).GetField("textColor",
                BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.NonPublic);
            Dictionary<int, Dictionary<int, string>> textColor =
                (Dictionary<int, Dictionary<int, string>>)textColor0.GetValue(GetSprites.instance);
            textColor0.SetValue(GetSprites.instance, textColor);
            List<int> textColorKeys = new List<int>(textColor.Keys);

            string text = File.OpenText(path).ReadToEnd();
            // �ļ����ݰ��в��
            string[] lineArray = text.Replace("\r", "").Split("\n"[0]);
            // ��һ��Ϊkey
            string[] lineIndex = lineArray[0].Split(',');
            for (int i = 1; i < lineArray.Length; i++)
            {
                // ���ÿһ��
                string[] dateBody = lineArray[i].Split(',');
                var id = dateBody[0];
                if (id != "#" && id != "")
                {
                    Dictionary<int, string> dateBodydic = new Dictionary<int, string>();
                    for (int j = 0; j < lineIndex.Length; j++)
                    {
                        bool flag3 = lineIndex[j] != "#" && lineIndex[j] != "" &&
                                     int.Parse(lineIndex[j]) != passDateIndex;
                        if (flag3)
                        {
                            bool flag4 = dateBody[j].Contains("C_");
                            if (flag4)
                            {
                                dateBody[j] = dateBody[j].Replace("C_D", "</color>");
                                while (dateBody[j].Contains("C_"))
                                {
                                    string textColorKey = Regex.Match(dateBody[j], "C_[1-9][0-9]*").ToString();
                                    if (textColorKeys.Contains(textColorKey.Substring(2).ParseInt()))
                                    {
                                        dateBody[j] = dateBody[j].Replace(textColorKey, textColor[textColorKey.Substring(2).ParseInt()][0]);
                                    }
                                }
                            }
                            dateBodydic.Add(int.Parse(lineIndex[j]), Regex.Unescape(dateBody[j]));
                        }
                    }

                    DateList.Add(int.Parse(id), dateBodydic);
                }
            }// end for
            return true;
        }   // End of LoadBaseDate

        /// <summary>
        /// ���빦����Ч �����ع�����Чid => [������Чid, ������Чid] ��ӳ��
        /// </summary>
        /// <param name="root"></param>
        /// <param name="name"></param>
        /// <param name="setantipower"></param>
        /// <param name="antiname"></param>
        /// <returns></returns>
        public static Dictionary<int, List<int>> LoadGongFaPower(string root, string name,
            bool setantipower = false, string antiname = "")
        {
            Dictionary<int, Dictionary<int, string>> power; /*= new Dictionary<int, Dictionary<int, string>>();*/
            Dictionary<int, Dictionary<int, string>> antipower = new Dictionary<int, Dictionary<int, string>>();
            Dictionary<int, List<int>> idRemap = new Dictionary<int, List<int>>();
            if (LoadBaseDate(root, name, out power) &&
                (!setantipower || LoadBaseDate(root, antiname, out antipower)))
            {
                // ժ��һ������Keyֵ 97 �� ������ Dictionary<int ,string>�������ڼ����ֵ
                Dictionary<int, Dictionary<int, string>> oldDate = new Dictionary<int, Dictionary<int, string>>();
                foreach (var dictionary in DateFile.instance.gongFaFPowerDate.Values)
                {
                    if (dictionary.Keys.Contains(97))
                    {
                        oldDate.Add(1, dictionary);
                        break;
                    }
                }

                if (!IndexCheck("gongFaFPowerDate", power, oldDate) ||
                    !IndexCheck("gongFaFPowerDate", antipower, oldDate))
                {
                    Main.Logger.Log("���빦��Ч��ʧ��");
                    return idRemap;
                }

                int maxid = GetMaxid(DateFile.instance.gongFaFPowerDate, 5000);
                lock (DateFile.instance.gongFaFPowerDate)
                {
                    foreach (int id in power.Keys)
                    {
                        DateFile.instance.gongFaFPowerDate.Add(id + maxid, power[id]);
                        // List<int> [0]Ϊ������Ч���滻id [1]Ϊ������Ч���滻id
                        idRemap.Add(id, new List<int> { id + maxid, 0 });
                        if (antipower.Keys.Contains(id))
                        {
                            DateFile.instance.gongFaFPowerDate.Add(id + maxid + 5000, antipower[id]);
                            idRemap[id][1] = id + maxid + 5000;
                        }
                    }
                }
            }
            else Main.Logger.Log("��������" + name + "&" + antiname + "ʧ�ܣ������ļ����Ƿ���ȷ");

            return idRemap;
        }   // End of LoadGongFaPower

        /// <summary>
        /// ���빦����Ϣ������LoadGongFaPower���������Ч�������ع���id ����ӳ��
        /// </summary>
        /// <param name="root"></param>
        /// <param name="name"></param>
        /// <param name="power"></param>
        /// <param name="baseid"></param>
        /// <returns></returns>
        public static Dictionary<int, int> LoadGongFa(string root, string name, Dictionary<int, List<int>> power,
            int baseid = 0)
        {
            Dictionary<int, Dictionary<int, string>> gongfa;/* = new Dictionary<int, Dictionary<int, string>>();*/
            Dictionary<int, int> idRemap = new Dictionary<int, int>();
            if (LoadBaseDate(root, name, out gongfa))
            {
                if (!IndexCheck("gongFaDate", gongfa, DateFile.instance.gongFaDate))
                {
                    Main.Logger.Log("���빦��ʧ��");
                    return idRemap;
                }

                int maxid = 0;
                if (baseid == 0)
                {
                    maxid = GetMaxid(DateFile.instance.gongFaDate);
                }
                else maxid = baseid;

                lock (DateFile.instance.gongFaFPowerDate)
                {
                    foreach (int gongid in gongfa.Keys)
                    {
                        if (power.Keys.Contains(gongid))
                        {
                            gongfa[gongid][103] = power[gongid][0].ToString();
                            gongfa[gongid][104] = power[gongid][1].ToString();
                            Main.Logger.Log("���빦����" + gongfa[gongid][0] + " ����Id��" + (gongid + maxid) + " ����Ч��:" +
                                            power[gongid][0] + " ����Ч����" + power[gongid][1]);
                        }

                        DateFile.instance.gongFaDate.Add(gongid + maxid, gongfa[gongid]);
                        idRemap.Add(gongid, gongid + maxid);
                    }
                }
            }
            else Main.Logger.Log("��������" + name + "ʧ�ܣ������ļ����Ƿ���ȷ");

            return idRemap;
        }   // End of LoadGongFa

        public static Dictionary<int, int> LoadOtherDate(string root, string name,
            ref Dictionary<int, Dictionary<int, string>> DateList, int preindex = -1, bool indexcheck = false)
        {
            Dictionary<int, Dictionary<int, string>> data;/* = new Dictionary<int, Dictionary<int, string>>();*/
            Dictionary<int, int> changeid = new Dictionary<int, int>();
            if (LoadBaseDate(root, name, out data, preindex))
            {
                if (IndexCheck(name, data, DateList) || !indexcheck)
                {
                    int maxid = GetMaxid(DateList);
                    lock (DateList)
                    {
                        foreach (int id in data.Keys)
                        {
                            DateList.Add(id + maxid, data[id]);
                            changeid.Add(id, id + maxid);
                        }
                    }
                }
            }
            else Main.Logger.Log("��������" + name + "ʧ�ܣ������ļ����Ƿ���ȷ");

            return changeid;
        }   // End of LoadOtherDate

        public static void LoadFameDate(string root, string name)
        {
            if (DateLoader.LoadBaseDate(root, name, out Dictionary<int, Dictionary<int, string>> FameDate) &&
                DateLoader.IndexCheck("FameDate", FameDate, DateFile.instance.actorFameDate))
            {
                int cnt = 0;
                lock (DateFile.instance.actorFameDate)
                {
                    foreach (int id in FameDate.Keys)
                    {
                        if (!DateFile.instance.actorFameDate.Keys.Contains(id))
                        {
                            DateFile.instance.actorFameDate.Add(id, FameDate[id]);
                            cnt++;
                            Main.Logger.Log($"�ɹ���������Ӱ��{FameDate[id][0]}, ID:{id}.");
                        }
                    }
                }
                Main.Logger.Log($"�ɹ�����{cnt}/{FameDate.Keys.ToArray().Length}������Ӱ��");
            }
            else
                Main.Logger.Log($"����������ʧ��");
        }

        public static Dictionary<int, int> LoadTipsMassage(string root, string name)
        {
            if (DateLoader.LoadBaseDate(root, name, out Dictionary<int, Dictionary<int, string>> TipsMassageDate)
                //&& DateLoader.IndexCheck("TipsMassageDate", TipsMassageDate, DateFile.instance.actorFameDate)
                )
            {
                Dictionary<int, int> idRemap = new Dictionary<int, int>();
                int cnt = 0;
                int maxid = GetMaxid(DateFile.instance.tipsMassageDate);
                lock (DateFile.instance.tipsMassageDate)
                {
                    foreach (int id in TipsMassageDate.Keys)
                    {
                        idRemap.Add(id, id + maxid);
                        if (!DateFile.instance.tipsMassageDate.Keys.Contains(idRemap[id]))
                        {
                            DateFile.instance.tipsMassageDate.Add(idRemap[id], TipsMassageDate[id]);
                            cnt++;
                        }
                    }
                }
                Main.Logger.Log($"�ɹ�����{cnt}/{TipsMassageDate.Keys.ToArray().Length}����ʾ��Ϣ");
            }
            else
                Main.Logger.Log($"������ʾ��Ϣʧ��");
            return null;
        }

        public static Dictionary<string, Sprite> SpriteCache = new Dictionary<string, Sprite>();

        /// <summary>
        /// ��ȡ�ⲿͼƬ
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Sprite CreateSpriteFromImage(string path)
        {
            if (!File.Exists(path))
            {
                Main.Logger.Log($"[Texture] Texture file {path}  NOT found.");
                return null;
            }

            byte[] fileData = File.ReadAllBytes(path);
            var toload = new Texture2D(2, 2);
            toload.LoadImage(fileData);
            var newsprite = Sprite.Create(toload, new Rect(0, 0, toload.width, toload.height), new Vector2(0, 0), 100);
            Main.Logger.Log($"[Texture] new Texture file {path} loaded, tex size : ({toload.width},{toload.height}).");
            return newsprite;
        }   // End of CreateSpriteFromImage

        /// <summary>
        /// ���� idRemap ����ӳ�� picList �� id
        /// </summary>
        /// <param name="picList"></param>
        /// <param name="idRemap"></param>
        /// <returns></returns>
        public static Dictionary<int, string> RemapIndex(Dictionary<int, string> picList, Dictionary<int, int> idRemap)
        {
            Dictionary<int, string> trans = new Dictionary<int, string>();
            foreach (int id in picList.Keys)
            {
                if (idRemap.Keys.Contains(id))
                {
                    trans.Add(idRemap[id], picList[id]);
                }
                else trans.Add(id, picList[id]);    // Ӧ�ò��ᱻִ�е�һ��
            }
            return trans;
        }   // End of RemapIndex

        /// <summary>
        /// Ϊ�¼�ָ��ͼƬ     BindImageFor_EventDate
        /// </summary>
        /// <param name="root"></param>
        /// <param name="piclist"></param>
        /// <param name="SpritesList"></param>
        /// <param name="DateList"></param>
        /// <param name="picindex">ָ��DateList[i] �б�ʾ picid���±�</param>
        public static void BindImageFor_EventDate(string root, Dictionary<int, string> piclist, ref Sprite[] SpritesList,
            ref Dictionary<int, Dictionary<int, string>> DateList, int picindex)
        {
            var img = SpritesList;
            if (img == null || img.Length == 0) return;
            foreach (int id in piclist.Keys)
            {
                string path = Path.Combine(root, piclist[id]);
                if (!File.Exists(path))
                {
                    Main.Logger.Log($"����ͼƬ {path} �����ڣ�����·�����ļ����Ƿ���ȷ");
                    continue;
                }

                var fileData = File.ReadAllBytes(path);
                var texture = new Texture2D(2, 2);
                texture.LoadImage(fileData);
                var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0), 100);
                if (sprite != null)
                {
                    var images = SpritesList;
                    SpritesList = images.AddToArray(sprite);
                    int num = SpritesList.Length - 1;

                    Main.Logger.Log("�ҵ�ͼƬID��" + num);
                    if (DateList.ContainsKey(id))
                    {
                        DateList[id][picindex] = num.ToString();
                        Main.Logger.Log("ָ���¼���" + id + "--->ͼƬID" + num + "���ɹ�");
                    }
                    else Main.Logger.Log("�����¼������ڣ�");
                }
                else Main.Logger.Log("����ͼƬ��Դ��ȡʧ��");
            }
        }   // End of BindImageFor_EventDate


        public static List<string> turnEventSpriteNameList = new List<string>();

        /// <summary>
        /// Ϊ�����¼���ͼƬ
        /// </summary>
        /// <param name="root"></param>
        /// <param name="turnEventImageName"></param>
        /// <param name="turnEventDateValue"></param>
        /// <param name="turnEventIndex"></param>
        public static void BindImageFor_TurnEventDate(string root, string turnEventImageName, Dictionary<int, string> turnEventDateValue,
            int turnEventIndex)
        {
            var dynamicSetSprite = SingletonObject.getInstance<DynamicSetSprite>();
            var getSpritesInfoAsset =
                (GetSpritesInfoAsset)Traverse.Create(dynamicSetSprite).Field("gsInfoAsset").GetValue();

            int spriteId = 0;

            string spritePath = Path.Combine(root, turnEventImageName);
            string spriteName = Path.GetFileNameWithoutExtension(spritePath);

            if (getSpritesInfoAsset.trunEventImage.Last() != spriteName)
            {
                GetSpritesInfoAsset.SingleSpritePathInfo spritePathInfo = new GetSpritesInfoAsset.SingleSpritePathInfo
                {
                    name = spriteName,
                    path = spritePath
                };
                getSpritesInfoAsset.singleSpritePathInfos.Add(spritePathInfo);
                Traverse.Create(getSpritesInfoAsset).Method("InitSingleSpritePathes").GetValue();

                getSpritesInfoAsset.trunEventImage =
                    getSpritesInfoAsset.trunEventImage.AddToArray(spriteName);

                var commonNameGroup =
                    (Dictionary<string, string[]>)Traverse.Create(getSpritesInfoAsset).Field("commonNameGroup").GetValue();
                commonNameGroup["trunEventImage"] = getSpritesInfoAsset.trunEventImage;
                spriteId = getSpritesInfoAsset.trunEventImage.Length - 1;
                Main.Logger.Log("��������¼�ͼƬ��" + turnEventImageName + " ͼƬId��" + spriteId);

                turnEventSpriteNameList.Add(spriteName);
                SpriteCache[spriteName] = CreateSpriteFromImage(spritePath); //���ⲿͼƬ���뻺��
            }

            if (spriteId == 0)
            {
                spriteId = getSpritesInfoAsset.trunEventImage.Length - 1;
            }

            if (turnEventDateValue.Keys.Contains(98))
            {
                turnEventDateValue[98] = spriteId.ToString();
            }
            else
            {
                Main.Logger.Log("��������¼�ʧ��");
            }
            int turnEventId = DateFile.instance.trunEventDate.Keys.Max() + 1;
            DateFile.instance.trunEventDate.Add(turnEventId, turnEventDateValue);
            Index.TurnEvenIndex[turnEventIndex] = turnEventId;

            Main.Logger.Log("��������¼���" + turnEventDateValue[0] + " �¼�Id��" + turnEventId + " ͼƬId��" + turnEventDateValue[98]);
        }   // End of BindImageFor_TurnEventDate

        /// <summary>
        /// ע������¼� ��ʱ�����ο�
        /// </summary>
        /// <param name="root"></param>
        /// <param name="txtname"></param>
        public static void Register(string root, string txtname)
        {
            Dictionary<int, Dictionary<int, string>> rawTurnEventDate;/* = new Dictionary<int, Dictionary<int, string>>();*/
            // ��������¼�
            LoadBaseDate(root, txtname, out rawTurnEventDate);
            if (!IndexCheck("trunEventDate", rawTurnEventDate, DateFile.instance.trunEventDate))
            {
                Main.Logger.Log("ע������¼�ͼƬʧ��");
                return;
            }

            List<Dictionary<int, string>> turnEventDateValues = new List<Dictionary<int, string>>();
            foreach (var key in rawTurnEventDate.Keys)
            {
                turnEventDateValues.Add(rawTurnEventDate[key]);
            }
            string img_folder = Path.Combine(root, "img");
            BindImageFor_TurnEventDate(img_folder, "StarFall_1.png", turnEventDateValues[0], 1);
            BindImageFor_TurnEventDate(img_folder, "StarFall_1.png", turnEventDateValues[1], 2);
            BindImageFor_TurnEventDate(img_folder, "StarFall_2.png", turnEventDateValues[2], 3);
            BindImageFor_TurnEventDate(img_folder, "StarFall_3.png", turnEventDateValues[3], 4);
            BindImageFor_TurnEventDate(img_folder, "SevenNine_0.png", turnEventDateValues[4], 5);
        }

        public static bool IsRegistered(string turnEventName)
        {
            var dynamicSetSprite = SingletonObject.getInstance<DynamicSetSprite>();
            var getSpritesInfoAsset =
                (GetSpritesInfoAsset)Traverse.Create(dynamicSetSprite).Field("gsInfoAsset").GetValue();
            return getSpritesInfoAsset.trunEventImage.Contains(turnEventName);
        }

    }   // End of EveningTwilight
}   // namespace of DateLoader
