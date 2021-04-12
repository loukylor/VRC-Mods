using System;
using MelonLoader;

namespace PlayerList.Config 
{
    public class EntryWrapper
    {
        public event Action OnValueChangedUntyped;

        protected void RunOnValueChangedUntyped() => OnValueChangedUntyped?.Invoke();
    }
    public class EntryWrapper<T> : EntryWrapper
    {
        public MelonPreferences_Entry<T> pref;

        public T Value
        {
            get { return pref.Value; }
            set 
            {
                if (!value.Equals(Value))
                {
                    OnValueChanged?.Invoke(pref.Value, value);
                    pref.Value = value;
                    RunOnValueChangedUntyped();
                }
            }
        }

        public event Action<T, T> OnValueChanged;
    }
}
