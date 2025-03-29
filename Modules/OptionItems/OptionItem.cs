using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BanMod
{
    public abstract class OptionItem
    {
        #region static
        public static IReadOnlyList<OptionItem> AllOptions => _allOptions;
        private static List<OptionItem> _allOptions = new(1024);
        public static IReadOnlyList<OptionItem> SettingOptions => _settingOptions;
        private static List<OptionItem> _settingOptions = new(512);
        public static IReadOnlyList<OptionItem> DenynameOptions => _denynameOptions;
        private static List<OptionItem> _denynameOptions = new(512);
        public static IReadOnlyList<OptionItem> BanlistOptions => _banlistOptions;
        private static List<OptionItem> _banlistOptions = new(512);
        public static IReadOnlyList<OptionItem> SpamlistOptions => _spamlistOptions;
        private static List<OptionItem> _spamlistOptions = new(512);
        public static IReadOnlyList<OptionItem> WordlistOptions => _wordlistOptions;
        private static List<OptionItem> _wordlistOptions = new(512);
        public static IReadOnlyDictionary<int, OptionItem> FastOptions => _fastOptions;
        private static Dictionary<int, OptionItem> _fastOptions = new(1024);
        public static int CurrentPreset { get; set; }
#if DEBUG
        public static bool IdDuplicated { get; private set; } = false;
#endif
        #endregion

        public int Id { get; }
        public string Name { get; }
        public int DefaultValue { get; }
        public TabGroup Tab { get; }
        public bool IsSingleValue { get; }

        public Color NameColor { get; protected set; }
        public OptionFormat ValueFormat { get; protected set; }
        public bool IsHeader { get; protected set; }
        public bool IsHidden { get; protected set; }
        public Dictionary<string, string> ReplacementDictionary
        {
            get => _replacementDictionary;
            set
            {
                if (value == null) _replacementDictionary?.Clear();
                else _replacementDictionary = value;
            }
        }
        private Dictionary<string, string> _replacementDictionary;

        public int[] AllValues { get; private set; } = new int[NumPresets];
        public int CurrentValue
        {
            get => GetValue();
            set => SetValue(value);
        }
        public int SingleValue { get; private set; }

        public OptionItem Parent { get; private set; }
        public static object ApplyDenyNameList { get; internal set; }

        public List<OptionItem> Children;

        public StringOption OptionBehaviour;

        public event EventHandler<UpdateValueEventArgs> UpdateValueEvent;

        public OptionItem(int id, string name, int defaultValue, TabGroup tab, bool isSingleValue)
        {
            Id = id;
            Name = name;
            DefaultValue = defaultValue;
            Tab = tab;
            IsSingleValue = isSingleValue;

            NameColor = Color.white;
            ValueFormat = OptionFormat.None;
            IsHeader = false;
            IsHidden = false;

            Children = new();

            if (Id == PresetId)
            {
                SingleValue = DefaultValue;
                CurrentPreset = SingleValue;
            }
            else if (IsSingleValue)
            {
                SingleValue = DefaultValue;
            }
            else
            {
                for (int i = 0; i < NumPresets; i++)
                {
                    AllValues[i] = DefaultValue;
                }
            }

            if (_fastOptions.TryAdd(id, this))
            {
                _allOptions.Add(this);
                switch (tab)
                {
                    case TabGroup.Banlist: _banlistOptions.Add(this); break;
                    case TabGroup.Spamlist: _spamlistOptions.Add(this); break;
                    case TabGroup.Wordlist: _wordlistOptions.Add(this); break;
                    default: Logger.Warn($"Encountered unknown option category \"{tab}\" (ID: {id}, Name: {name})", nameof(OptionItem)); break;
                }
            }
            else
            {
                Logger.Error($"ID:{id}が重複しています", "OptionItem");
            }
        }

        public OptionItem Do(Action<OptionItem> action)
        {
            action(this);
            return this;
        }

        public OptionItem SetColor(Color value) => Do(i => i.NameColor = value);
        public OptionItem SetValueFormat(OptionFormat value) => Do(i => i.ValueFormat = value);
        public OptionItem SetHeader(bool value) => Do(i => i.IsHeader = value);
        public OptionItem SetHidden(bool value) => Do(i => i.IsHidden = value);

        public OptionItem SetParent(OptionItem parent) => Do(i =>
        {
            i.Parent = parent;
            parent.SetChild(i);
        });
        public OptionItem SetChild(OptionItem child) => Do(i => i.Children.Add(child));
        public OptionItem RegisterUpdateValueEvent(EventHandler<UpdateValueEventArgs> handler)
            => Do(i => UpdateValueEvent += handler);

        public OptionItem AddReplacement((string key, string value) kvp)
            => Do(i =>
            {
                ReplacementDictionary ??= new();
                ReplacementDictionary.Add(kvp.key, kvp.value);
            });
        public OptionItem RemoveReplacement(string key)
            => Do(i => ReplacementDictionary?.Remove(key));

        public virtual string GetName(bool disableColor = false)
        {
            return disableColor ?
                Translator.GetString(Name, ReplacementDictionary) :
                Utils.ColorString(NameColor, Translator.GetString(Name, ReplacementDictionary));
        }
        public virtual bool GetBool() => CurrentValue != 0 && (Parent == null || Parent.GetBool());
        public virtual int GetInt() => CurrentValue;
        public virtual float GetFloat() => CurrentValue;
        public virtual string GetString()
        {
            return ApplyFormat(CurrentValue.ToString());
        }
        public virtual int GetValue() => IsSingleValue ? SingleValue : AllValues[CurrentPreset];


        public string ApplyFormat(string value)
        {
            if (ValueFormat == OptionFormat.None) return value;
            return string.Format(Translator.GetString("Format." + ValueFormat), value);
        }

        public virtual void Refresh()
        {
            if (OptionBehaviour is not null and StringOption opt)
            {
                opt.TitleText.text = GetName();
                opt.ValueText.text = GetString();
                opt.oldValue = opt.Value = CurrentValue;
            }
        }
        public virtual void SetValue(int afterValue, bool doSave, bool doSync = true)
        {
            int beforeValue = CurrentValue;
            if (IsSingleValue)
            {
                SingleValue = afterValue;
            }
            else
            {
                AllValues[CurrentPreset] = afterValue;
            }

            CallUpdateValueEvent(beforeValue, afterValue);
            Refresh();
            if (doSync)
            {
                SyncAllOptions();
            }
        }
        public virtual void SetValue(int afterValue, bool doSync = true)
        {
            SetValue(afterValue, true, doSync);
        }
        public void SetAllValues(int[] values) 
        {
            AllValues = values;
        }

        public static OptionItem operator ++(OptionItem item)
            => item.Do(item => item.SetValue(item.CurrentValue + 1));
        public static OptionItem operator --(OptionItem item)
            => item.Do(item => item.SetValue(item.CurrentValue - 1));

        public static void SwitchPreset(int newPreset)
        {
            CurrentPreset = Math.Clamp(newPreset, 0, NumPresets - 1);

            foreach (var op in AllOptions)
                op.Refresh();

            SyncAllOptions();
        }
        public static void SyncAllOptions()
        {
            if (
                BanMod.AllPlayerControls.Count() <= 1 ||
                AmongUsClient.Instance.AmHost == false ||
                PlayerControl.LocalPlayer == null
            ) return;

        }

        private void CallUpdateValueEvent(int beforeValue, int currentValue)
        {
            if (UpdateValueEvent == null) return;
            try
            {
                UpdateValueEvent(this, new UpdateValueEventArgs(beforeValue, currentValue));
            }
            catch (Exception ex)
            {
                Logger.Error($"[{Name}] UpdateValueEventの呼び出し時に例外が発生しました", "OptionItem.UpdateValueEvent");
                Logger.Exception(ex, "OptionItem.UpdateValueEvent");
            }
        }

        public class UpdateValueEventArgs : EventArgs
        {
            public int CurrentValue { get; set; }
            public int BeforeValue { get; set; }
            public UpdateValueEventArgs(int beforeValue, int currentValue)
            {
                CurrentValue = currentValue;
                BeforeValue = beforeValue;
            }
        }

        public const int NumPresets = 5;
        public const int PresetId = 0;
    }

    public enum TabGroup
    {
        Banlist,
        Spamlist,
        Wordlist

    }
    public enum OptionFormat
    {
        None,
        Players,
        Seconds,
        Percent,
        Times,
        Multiplier,
        Votes,
        Pieces,
    }
}