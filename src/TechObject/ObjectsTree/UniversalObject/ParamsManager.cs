﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Editor;

namespace TechObject
{
    public class ParamsManager : TreeViewItem
    {
        /// <summary>
        /// Все параметры технологического объекта.
        /// </summary>
        public ParamsManager()
        {
            items = new List<ITreeViewItem>();

            parFLoat = new Params("Параметры float", "par_float", false,
                "S_PAR_F", true);
            parFLoat.Parent = this;
            items.Add(parFLoat);
        }

        /// <summary>
        /// Добавление параметра.
        /// </summary>
        /// <param name="group">Группа.</param>
        /// <param name="name">Имя.</param>
        /// <param name="value">Значение.</param>
        /// <param name="meter">Размерность.</param>
        /// <param name="nameLua">Имя в Lua.</param>
        public Param AddParam(string group, string name, float value,
            string meter, string nameLua = "")
        {
            Param res = null;
            switch (group)
            {
                case "par_float":
                    res = parFLoat.AddParam(
                        new Param(parFLoat.GetIdx, name, false, value, meter,
                        nameLua, true));
                    break;

                case "rt_par_float":
                    if(parFLoatRunTime == null)
                    {
                        parFLoatRunTime = new Params("Рабочие параметры float",
                            "rt_par_float", true, "RT_PAR_F");
                        parFLoatRunTime.Parent = this;
                        items.Add(parFLoatRunTime);
                    }

                    res = parFLoatRunTime.AddParam(
                        new Param(parFLoatRunTime.GetIdx, name, true, value,
                        meter, nameLua));
                    break;
            }

            return res;
        }

        /// <summary>
        /// Получение параметра.
        /// </summary>
        /// <param name="nameLua">Имя в Lua.</param>
        public Param GetParam(string nameLua)
        {
            return parFLoat.GetParam(nameLua);
        }

        /// <summary>
        /// Сохранение в виде таблицы Lua.
        /// </summary>
        /// <param name="prefix">Префикс (для выравнивания).</param>
        /// <returns>Описание в виде таблицы Lua.</returns>
        public string SaveAsLuaTable(string prefix)
        {
            string res = "";

            foreach(Params paramsGroups in Items)
            {
                res += paramsGroups.SaveAsLuaTable(prefix);
            }

            return res;
        }

        public ParamsManager Clone()
        {
            ParamsManager clone = (ParamsManager)MemberwiseClone();
            clone.items = new List<ITreeViewItem>();

            clone.parFLoat = parFLoat.Clone();
            clone.items.Add(clone.parFLoat);

            if (parFLoatRunTime != null)
            {
                clone.parFLoatRunTime = parFLoatRunTime.Clone();
                clone.items.Add(clone.parFLoatRunTime);
            }

            return clone;
        }

        /// <summary>
        /// Получить Float параметры объекта.
        /// </summary>
        public Params Float
        {
            get
            {
                return parFLoat;
            }
        }

        /// <summary>
        /// Проверить менеджер параметров объекта.
        /// </summary>
        /// <param name="objName">Имя объекта</param>
        /// <returns>Ошибки</returns>
        public string Check(string objName)
        {
            var errors = "";
            errors += Float.Check(objName);
            return errors;
        }

        public void Clear()
        {
            parFLoat.Clear();
            parFLoatRunTime.Clear();
        }

        #region Реализация ITreeViewItem
        override public string[] DisplayText
        {
            get
            {
                return new string[] { "Параметры", "" };
            }
        }

        override public ITreeViewItem[] Items
        {
            get
            {
                return items.ToArray();
            }
        }

        override public bool IsCopyable
        {
            get
            {
                return true;
            }
        }

        override public bool IsReplaceable
        {
            get
            {
                return true;
            }
        }

        override public bool Delete(object child)
        {
            Params params_ = child as Params;
            if (params_ != null)
            {
                params_.Clear();
                return true;
            }

            return false;
        }

        override public ITreeViewItem Replace(object child,
            object copyObject)
        {
            Params pars = child as Params;
            if (copyObject is Params && pars != null)
            {
                pars.Clear();
                Params copyPars = copyObject as Params;
                foreach (Param par in copyPars.Items)
                {
                    pars.InsertCopy(par);
                }

                return pars;
            }

            return null;
        }

        public override bool ShowWarningBeforeDelete
        {
            get
            {
                return true;
            }
        }

        public override ImageIndexEnum ImageIndex
        {
            get
            {
                return ImageIndexEnum.ParamsManager;
            }
        }
        #endregion

        public override string GetLinkToHelpPage()
        {
            string ostisLink = EasyEPlanner.ProjectManager.GetInstance()
                .GetOstisHelpSystemLink();
            return ostisLink + "?sys_id=process_parameter";
        }

        private Params parFLoat;
        private Params parFLoatRunTime;

        private List<ITreeViewItem> items;
    }
}