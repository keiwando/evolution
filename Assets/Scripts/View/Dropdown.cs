using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Keiwando.UI {

    public class Dropdown<T> where T: IEquatable<T> {

        public event Action<T> onValueChanged;

        public struct Data {
            public T Value { get; set; }
            public string Label { get; set; }
            public Sprite Sprite { get; set; }
        }

        private UnityEngine.UI.Dropdown dropdown;

        private List<Data> items;

        public Dropdown(UnityEngine.UI.Dropdown dropdown, List<Data> items) {
            this.dropdown = dropdown;
            this.items = items;
            dropdown.options = items.Select(
                x => new UnityEngine.UI.Dropdown.OptionData(x.Label, x.Sprite)
            ).ToList();

            dropdown.onValueChanged.AddListener(delegate (int index) {
                if (onValueChanged != null) {
                    onValueChanged(this.items[index].Value);
                }
            });
        }

        public void Refresh(T value) {

            for (int i = 0; i < items.Count; i++) {
                if (this.items[i].Value.Equals(value)) {
                    this.dropdown.value = i;
                }
            }
        }
    }
}