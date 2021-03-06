﻿using System.Collections.Generic;
using System.Linq;
using Editor;

namespace TechObject
{
    /// <summary>
    /// Класс реализующий базовую операцию для технологического объекта
    /// </summary>
    public class BaseOperation : TreeViewItem
    {
        public BaseOperation(Mode owner)
        {
            Name = "";
            LuaName = "";
            Properties = new List<BaseParameter>();
            Steps = new List<BaseStep>();
            this.owner = owner;
        }

        /// <summary>
        /// Возвращает пустой объект - базовая операция.
        /// </summary>
        /// <returns></returns>
        public static BaseOperation EmptyOperation()
        {
            return new BaseOperation("", "", new List<BaseParameter>(), 
                new List<BaseStep>());
        }

        /// <summary>
        /// Конструктор для инициализации базовой операции и параметров
        /// </summary>
        /// <param name="name">Имя операции</param>
        /// <param name="luaName">Lua имя операции</param>
        /// <param name="baseOperationProperties">Свойства операции</param>
        /// <param name="baseSteps">Базовые шаги операции</param>
        public BaseOperation(string name, string luaName, 
            List<BaseParameter> baseOperationProperties, 
            List<BaseStep> baseSteps)
        {
            Name = name;
            LuaName = luaName;
            Properties = baseOperationProperties;
            Steps = baseSteps;
        }

        /// <summary>
        /// Добавить базовый шаг
        /// </summary>
        /// <param name="luaName">Lua-имя</param>
        /// <param name="name">Имя</param>
        /// <param name="defaultPosition">Стандартная позиция шага в 
        /// базовой операции.</param>
        public void AddStep(string luaName, string name, int defaultPosition)
        {
            if (Steps.Count == 0)
            {
                var emptyStep = new BaseStep("", "");
                emptyStep.Owner = this;
                // Пустой объект, если не должно быть выбрано никаких объектов
                Steps.Add(emptyStep);
            }

            var step = new BaseStep(name, luaName, defaultPosition);
            step.Owner = this;
            Steps.Add(step);
        }

        /// <summary>
        /// Добавить активный параметр
        /// </summary>
        /// <param name="luaName">Lua-имя</param>
        /// <param name="name">Имя</param>
        /// <param name="defaultValue">Значение по-умолчанию</param>
        /// <returns>Добавленный параметр</returns>
        public ActiveParameter AddActiveParameter(string luaName, string name, 
            string defaultValue)
        {
            var par = new ActiveParameter(luaName, name, defaultValue);
            par.Owner = this;
            Properties.Add(par);
            return par;
        }
        
        /// <summary>
        /// Добавить активный булевый параметр
        /// </summary>
        /// <param name="luaName">Lua-имя</param>
        /// <param name="name">Имя</param>
        /// <param name="defaultValue">Значение по-умолчанию</param>
        public void AddActiveBoolParameter(string luaName, string name,
            string defaultValue)
        {
            var par = new ActiveBoolParameter(luaName, name,
                defaultValue);
            par.Owner = this;
            Properties.Add(par);
        }

        /// <summary>
        /// Получить имя операции
        /// </summary>
        public string Name
        {
            get
            {
                return operationName;
            }

            set
            {
                operationName = value;
            }
        }

        /// <summary>
        /// Получить Lua имя операции
        /// </summary>
        public string LuaName
        {
            get
            {
                return luaOperationName;
            }

            set
            {
                luaOperationName = value;
            }
        }

        /// <summary>
        /// Шаги операции.
        /// </summary>
        public List<BaseStep> Steps
        {
            get
            {
                return baseSteps;
            }
            set
            {
                baseSteps = value;
            }
        }

        /// <summary>
        /// Инициализация базовой операции по имени
        /// </summary>
        /// <param name="baseOperName">Имя операции</param>
        /// <param name="mode">Операция владелец</param>
        public void Init(string baseOperName, Mode mode)
        {
            TechObject techObject = owner.Owner.Owner;
            string baseTechObjectName = techObject.BaseTechObject.Name;

            ResetOperationSteps();

            if (baseTechObjectName != "")
            {
                BaseOperation operation;
                operation = techObject.BaseTechObject
                    .GetBaseOperationByName(baseOperName);
                if (operation == null)
                {
                    operation = techObject.BaseTechObject
                        .GetBaseOperationByLuaName(baseOperName);
                }

                if (operation != null)
                {
                    Name = operation.Name;
                    LuaName = operation.LuaName;
                    Properties = operation.Properties
                        .Select(x => x.Clone())
                        .ToList();
                    foreach(var property in Properties)
                    {
                        property.Owner = this;
                        property.Parent = this;
                    }

                    baseSteps = operation.Steps;
                    foreach(var step in baseSteps)
                    {
                        step.Owner = this;
                    }

                    owner = mode;
                    if(mode.Name == Mode.DefaultModeName)
                    {
                        mode.SetNewValue(operation.Name);
                    }
                }
            }
            else
            {
                Name = "";
                LuaName = "";
                baseOperationProperties = new List<BaseParameter>();
                baseSteps = new List<BaseStep>();
            }

            techObject.AttachedObjects.Check();
            SetItems();
        }

        /// <summary>
        /// Сбросить базовые шаги базовой операции
        /// </summary>
        private void ResetOperationSteps()
        {
            foreach (var step in owner.MainSteps)
            {
                step.SetNewValue("", true);
            }
        }

        /// <summary>
        /// Добавление полей в массив для отображения на дереве
        /// </summary>
        private void SetItems()
        {
            var showedParameters = new List<BaseParameter>();
            foreach (var parameter in Properties)
            {
                showedParameters.Add(parameter);
            }
            items = showedParameters.ToArray();
        }

        /// <summary>
        /// Сохранение в виде таблицы Lua
        /// </summary>
        /// <param name="prefix">Префикс (отступ)</param>
        /// <returns></returns>
        public string SaveAsLuaTable(string prefix)
        {
            var res = "";
            var propertiesCountForSave = Properties.Count();
            if (Properties == null || propertiesCountForSave <= 0)
            {
                return res;
            }

            string paramsForSave = "";
            foreach (var operParam in Properties)
            {
                if(!operParam.NeedDisable && !operParam.IsEmpty)
                {
                    paramsForSave += "\t" + prefix + operParam.LuaName +
                        " = \'" + operParam.Value + "\',\n";
                }
            }

            if (paramsForSave != "")
            {
                res += prefix + "props =\n" + prefix + "\t{\n";
                res += paramsForSave;
                res += prefix + "\t},\n";
            }

            return res;
        }

        /// <summary>
        /// Установка свойств базовой операции
        /// </summary>
        /// <param name="extraParams">Свойства операции</param>
        public void SetExtraProperties(ObjectProperty[] extraParams)
        {
            foreach (ObjectProperty extraParam in extraParams)
            {
                var property = Properties
                    .Where(x => x.LuaName.Equals(extraParam.DisplayText[0]))
                    .FirstOrDefault();

                if (property != null)
                {
                    property.SetValue(extraParam.Value);
                }
            }
        }

        /// <summary>
        /// Получить свойства базовой операции
        /// </summary>
        public List<BaseParameter> Properties
        {
            get
            {
                return baseOperationProperties;
            }
            set
            {
                baseOperationProperties = value;
            }
        }

        public Mode Owner
        {
            get
            {
                return owner;
            }
        }

        /// <summary>
        /// Номер операции в списке операций объекта. Если 0 - не задано.
        /// </summary>
        public int DefaultPosition { get; set; } = 0;
       
        /// <summary>
        /// Добавить свойства базовой операции.
        /// </summary>
        /// <param name="properties">Массив свойств</param>
        /// <param name="owner">Объект первоначальный владелец свойств</param>
        public void AddProperties(List<BaseParameter> properties, object owner)
        {
            foreach(var property in properties)
            {
                var equalPropertiesCount = Properties
                    .Where(x => x.LuaName == property.LuaName).Count();
                if (equalPropertiesCount == 0)
                {
                    var newProperty = property.Clone();
                    newProperty.Owner = owner;
                    newProperty.Parent = this;
                    Properties.Add(newProperty);
                }
            }

            SetItems();
        }

        /// <summary>
        /// Удалить свойства базовой операции.
        /// </summary>
        /// <param name="properties">Массив свойств</param>
        public void RemoveProperties(List<BaseParameter> properties)
        {
            foreach (var property in properties)
            {
                var deletingProperty = Properties
                    .Where(x => x.LuaName == property.LuaName)
                    .FirstOrDefault();
                if (deletingProperty != null)
                {
                    Properties.Remove(deletingProperty);
                }
            }
            SetItems();
        }

        /// <summary>
        /// Изменить владельца свойств агрегата в аппарате.
        /// </summary>
        /// <param name="oldOwner">Старый базовый объект</param>
        /// <param name="newOwner">Новый базовый объект</param>
        public void ChangePropertiesOwner(BaseTechObject oldOwner,
            BaseTechObject newOwner)
        {
            foreach(var property in Properties)
            {
                if (property.Owner == oldOwner)
                {
                    property.Owner = newOwner;
                }
            }
        }

        /// <summary>
        /// Проверка базовой операции
        /// </summary>
        public string Check()
        {
            string errors = "";
            foreach (var property in Properties)
            {
                if (property is MainAggregateParameter)
                {
                    (property as MainAggregateParameter).Check();
                }

                bool notStub = !property.Value.ToLower()
                    .Contains(StaticHelper.CommonConst.StubForCells
                    .ToLower());
                if (notStub)
                {
                    CheckNotEmptyDisabledAggregateProperties(property,
                        ref errors);
                }
            }

            return errors;
        }

        /// <summary>
        /// Проверка пустых не отключенных параметров агрегатов
        /// </summary>
        /// <param name="property">Свойство</param>
        /// <param name="errors">Список ошибок</param>
        private void CheckNotEmptyDisabledAggregateProperties(
            BaseParameter property, ref string errors)
        {
            bool notEmptyDisabledAggregateProperty =
                property.Owner is BaseTechObject &&
                !property.Disabled &&
                (property.Value == "");
            if (notEmptyDisabledAggregateProperty)
            {
                string modeName = owner.DisplayText[0];
                string techObjName = Owner.Owner.Owner.DisplayText[0];
                string message = $"Свойство \"{property.Name}\" в " +
                    $"операции \"{modeName}\", объекта \"{techObjName}\"" +
                    $" не заполнено.\n";
                errors += message;
            }
        }

        /// <summary>
        /// Копирование объекта
        /// </summary>
        /// <param name="owner">Новая операция-владелец объекта</param>
        /// <returns></returns>
        public BaseOperation Clone(Mode owner)
        {
            var operation = Clone();
            operation.owner = owner;
            return operation;
        }

        /// <summary>
        /// Копирование объекта
        /// </summary>
        /// <returns></returns>
        public BaseOperation Clone()
        {
            var properties = new List<BaseParameter>(baseOperationProperties
                .Count);
            var operation = EmptyOperation();
            for (int i = 0; i < baseOperationProperties.Count; i++)
            {
                BaseParameter oldProperty = baseOperationProperties[i];
                BaseParameter newProperty = oldProperty.Clone();
                if (oldProperty.Owner is BaseTechObject)
                {
                    var obj = oldProperty.Owner as BaseTechObject;
                    if (obj.IsAttachable)
                    {
                        newProperty.Owner = oldProperty.Owner;
                    }
                }
                else
                {
                    newProperty.Owner = operation;
                }
                properties.Add(newProperty);
            }

            var steps = new List<BaseStep>();
            for (int i = 0; i < Steps.Count; i++)
            {
                var newStep = Steps[i].Clone();
                newStep.Owner = operation;
                steps.Add(newStep);
            }

            operation.Name = operationName;
            operation.LuaName = luaOperationName;
            operation.Properties = properties;
            operation.Steps = steps;
            operation.owner = this.Owner;
            operation.DefaultPosition = DefaultPosition;

            operation.SetItems();

            return operation;
        }

        #region синхронизация устройств
        public void Synch(int[] array)
        {
            foreach(var property in baseOperationProperties)
            {
                property.Synch(array);
            }
        }
        #endregion

        #region Реализация ITreeViewItem
        override public string[] DisplayText
        {
            get
            {
                if (items.Count() > 0)
                {
                    string res = string.Format("Доп. свойства ({0})", 
                        items.Count());
                    return new string[] { res, "" };
                }
                else
                {
                    string res = string.Format("Доп. свойства");
                    return new string[] { res, "" };
                }
            }
        }

        override public ITreeViewItem[] Items
        {
            get
            {
                return items;
            }
        }

        public override bool Delete(object child)
        {
            if (child is ActiveParameter)
            {
                var property = child as ActiveParameter;
                property.SetNewValue("");
                return true;
            }
            return false;
        }

        public override bool IsFilled
        {
            get
            {
                if(items.Where(x=> x.IsFilled).Count() > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        #endregion

        public override string GetLinkToHelpPage()
        {
            string ostisLink = EasyEPlanner.ProjectManager.GetInstance()
                .GetOstisHelpSystemLink();
            return ostisLink + "?sys_id=process_parameter";
        }

        private ITreeViewItem[] items = new ITreeViewItem[0];
        
        private List<BaseParameter> baseOperationProperties;
        private string operationName;
        private string luaOperationName;
        private List<BaseStep> baseSteps;

        private Mode owner;
    }
}
