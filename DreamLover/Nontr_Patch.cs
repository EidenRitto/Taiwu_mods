using GameData;
using Harmony12;
using System.Collections.Generic;
using UnityEngine;

namespace DreamLover
{
    public static class Nontr_Patch
    {
        public static void Debug(string info)
        {
            Main.Debug("<����> " + info);
        }

        public static PatchModuleInfo patchModuleInfo = new PatchModuleInfo(
            typeof(PeopleLifeAI), "AISetChildren",
            typeof(Nontr_Patch));

        public static bool Prefix(int fatherId, int motherId, int setFather, int setMother, ref bool __result)
        {
            if (!Main.enabled)
            {
                return true;
            }
            int mainActorId = DateFile.instance.MianActorID();

            // ���ù��ܲ��Ҳ�������
            if (Main.settings.nontr.Enabled && fatherId != mainActorId && motherId != mainActorId)
            {
                bool �Ƿ����� = false;

                bool �����Ƿ���Ҫ���� = false;

                if (Main.settings.nontr.PreventAll)
                    �����Ƿ���Ҫ���� = true;

                List<int> list1 = DateFile.instance.GetActorSocial(fatherId, 309);
                List<int> list2 = DateFile.instance.GetActorSocial(motherId, 309);
                bool ��̫��Ĺ�ϵ�� = false;
                foreach (int i in list1)
                    if (!��̫��Ĺ�ϵ�� && DateFileHelper.HasAnySocial(mainActorId, Main.NoNtrSocialTypList, i))
                        ��̫��Ĺ�ϵ�� = true;
                foreach (int i in list2)
                    if (!��̫��Ĺ�ϵ�� && DateFileHelper.HasAnySocial(mainActorId, Main.NoNtrSocialTypList, i))
                        ��̫��Ĺ�ϵ�� = true;

                bool �Ƿ���ż��ϵ = DateFileHelper.HasSocial(fatherId, 309, motherId);

                if (��̫��Ĺ�ϵ��)
                    �����Ƿ���Ҫ���� = true;

                �Ƿ����� = �����Ƿ���Ҫ����;
                if (Main.settings.nontr.AllowCouple && �Ƿ���ż��ϵ) �Ƿ����� = false;

                if (�Ƿ�����)
                {
                    if (��̫��Ĺ�ϵ��)
                    {
                        Debug(string.Format("��⵽��Ҫ������� {0} {1} ���ϵ�б��ڵ����л�����ϵ��������", DateFile.instance.GetActorName(fatherId), DateFile.instance.GetActorName(motherId)));
                    }
                    else
                    {
                        Debug(string.Format("��⵽��Ҫ������� {0} {1} ���ˣ�������", DateFile.instance.GetActorName(fatherId), DateFile.instance.GetActorName(motherId)));
                    }
                }
                else
                {
                    if (��̫��Ĺ�ϵ��)
                    {
                        Debug(string.Format("��⵽��Ҫ������� {0} {1} ���ϵ�б��ڵ����л�����ϵ��δ����", DateFile.instance.GetActorName(fatherId), DateFile.instance.GetActorName(motherId)));
                    }
                }

                return !�Ƿ�����;
            }
            else
            {
                // ������
                return true;
            }
        }
    }
}
