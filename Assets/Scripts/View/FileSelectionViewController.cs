using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Keiwando.UI;
using Keiwando;

namespace Keiwando.Evolution.UI {

	public interface FileSelectionViewControllerDelegate {

		string GetTitle(FileSelectionViewController controller);
		int GetNumberOfItems(FileSelectionViewController controller);
		string GetTitleForItemAtIndex(FileSelectionViewController controller,
									int index);
		string GetEmptyMessage(FileSelectionViewController controller);
		int GetIndexOfSelectedItem(FileSelectionViewController controller);

		void DidSelectItem(FileSelectionViewController controller, int index);

		bool IsCharacterValidForName(FileSelectionViewController controller, char c);
		bool IsNameAvailable(FileSelectionViewController controller, string newName);
		void DidEditTitleAtIndex(FileSelectionViewController controller, int index, string newName);

		void LoadButtonClicked(FileSelectionViewController controller);
		void ImportButtonClicked(FileSelectionViewController controller);
		void ExportButtonClicked(FileSelectionViewController controller);
		void DeleteButtonClicked(FileSelectionViewController controller);
		void DidClose(FileSelectionViewController controller);
	}

	public class FileSelectionViewController : MonoBehaviour, SelectableListItemViewDelegate,
											RenameDialogDelegate {

		private FileSelectionViewControllerDelegate controllerDelegate;

		[SerializeField]
		private Text titleLabel;

		[SerializeField]
		private ScrollRect scrollRect;
		[SerializeField]
		private ListLayoutGroup itemList;

		[SerializeField]
		private SelectableListItemView itemTemplate;

		private List<SelectableListItemView> items = new List<SelectableListItemView>();

		[SerializeField]
		private Text emptyMessageLabel;
		[SerializeField]
		private Text currentSelectionLabel;

		[SerializeField]
		private DeleteConfirmationDialog deleteConfirmation;
		[SerializeField]
		private RenameDialog renameDialog;

		[SerializeField]
		private TMPro.TMP_InputField searchInputField;

		private Search search;
		private SearchResult searchResult;

		[SerializeField]
		private RectTransform visibleContainerRect;
		private RectTransform containerRect;

		public void Show(FileSelectionViewControllerDelegate Delegate) {
			this.controllerDelegate = Delegate;

			this.containerRect = itemTemplate.transform.parent.GetComponent<RectTransform>();

			InputRegistry.shared.Register(InputType.All, this, EventHandleMode.ConsumeEvent);
			GestureRecognizerCollection.shared.GetAndroidBackButtonGestureRecognizer().OnGesture += OnAndroidBackButton;

			DeleteAllItemViews();
			gameObject.SetActive(true);
			RefreshSearch();
			searchInputField.text = "";
			searchInputField.onValueChanged.AddListener(delegate (string value) {
				this.searchResult = this.search.Find(value);
				Refresh();
			});

			Refresh();
			ScrollSelectedItemToTop();
		}

		public void Refresh() {
			if (controllerDelegate == null) { return; }

			titleLabel.text = controllerDelegate.GetTitle(this);
			RefreshItemList();
			RefreshCurrentSelection();

			bool showEmptyMessage = items.Count == 0 && string.IsNullOrEmpty(searchInputField.text);
			if (showEmptyMessage) {
				emptyMessageLabel.text = controllerDelegate.GetEmptyMessage(this);
			}
			emptyMessageLabel.gameObject.SetActive(showEmptyMessage);
		}

		private void RefreshItemList() {

			var container = itemTemplate.transform.parent;
			// Create all necessary item views first
			int diff = GetNumberOfItems() - items.Count;

			if (diff < 0) {
				// Delete unneeded item views
				int index = System.Math.Max(0, items.Count + diff - 1);
				var toDelete = items.GetRange(index, -diff);
				items.RemoveRange(index, -diff);
				foreach (var item in toDelete) {
					Destroy(item.gameObject);
				}
			} else if (diff > 0) {
				// Create necessary item views
				itemTemplate.gameObject.SetActive(true);
				for (int i = 0; i < diff; i++) {
					var itemView = Instantiate(itemTemplate, container);
					itemView.Delegate = this;
					items.Add(itemView);
				}
				itemTemplate.gameObject.SetActive(false);
			}

			var size = containerRect.sizeDelta;
			size = new Vector2(size.x, items.Count * (itemList.CellHeight + itemList.Spacing));
			containerRect.sizeDelta = size;

			// Update item views
			int selectedIndex = IndexInSearchResult(controllerDelegate.GetIndexOfSelectedItem(this));
			for (int i = 0; i < items.Count; i++) {

				string title = GetTitleForItemAtIndex(i);
				items[i].Refresh(title, selectedIndex == i);
			}
		}

		private void RefreshCurrentSelection() {
			int selectedIndex = controllerDelegate.GetIndexOfSelectedItem(this);
			string title = controllerDelegate.GetTitleForItemAtIndex(this, selectedIndex);
			currentSelectionLabel.text = title;
		}

		private void ScrollSelectedItemToTop() {
			
			float itemCount = GetNumberOfItems();
			int currentSelected = controllerDelegate.GetIndexOfSelectedItem(this);
			
			if (IsIndexInSearchResult(currentSelected))
				// scrollRect.verticalNormalizedPosition = itemCount == 0 ? 1f : 1f - (float)currentSelected / itemCount;
				scrollRect.verticalNormalizedPosition = CalculateScrollPositionForItem(currentSelected);
		}

		private float CalculateScrollPositionForItem(int index) {
			
			float itemCount = GetNumberOfItems();
			if (itemCount == 0) return 1f;
			float itemHeight = itemList.CellHeight + itemList.Spacing;
			float itemsOnScreen = visibleContainerRect.rect.height / itemHeight;
			float relScrollPerItem = 1f / (itemCount - itemsOnScreen);

			return 1f - (float)index * relScrollPerItem;
		}

		// Search filter

		private void RefreshSearch() {
			int itemCount = controllerDelegate.GetNumberOfItems(this);
			string[] items = new string[itemCount];
			for (int i = 0; i < itemCount; i++) {
				items[i] = controllerDelegate.GetTitleForItemAtIndex(this, i);
			}
			this.search = new Search(items);
			this.searchResult = this.search.Find(searchInputField.text);
		}

		private int GetNumberOfItems() {
			if (string.IsNullOrEmpty(searchInputField.text)) {
				return this.controllerDelegate.GetNumberOfItems(this);
			} else {
				return this.searchResult.indices.Length;
			}
		}

		private string GetTitleForItemAtIndex(int index) {
			if (string.IsNullOrEmpty(searchInputField.text)) {
				return this.controllerDelegate.GetTitleForItemAtIndex(this, index);
			} else {
				return this.searchResult.results[index];
			}
		}

		private void DidSelectItemAtIndex(int index) {
			
			if (string.IsNullOrEmpty(searchInputField.text)) {
				this.controllerDelegate.DidSelectItem(this, index);
			} else {
				this.controllerDelegate.DidSelectItem(this, this.searchResult.indices[index]);
			}
		}

		private bool IsIndexInSearchResult(int index) {
			if (string.IsNullOrEmpty(searchInputField.text)) {
				return true;
			} else {
				return IndexInSearchResult(index) != -1;
			}
		}

		private int IndexInSearchResult(int index) {
			if (string.IsNullOrEmpty(searchInputField.text)) {
				return index;
			}
			var indices = this.searchResult.indices;
			for (int i = 0; i < this.searchResult.indices.Length; i++) {
				if (indices[i] == index) {
					return i;
				}
			}
			return -1;
		}


		public void Close() {
			controllerDelegate = null;
			InputRegistry.shared.Deregister(this);
			GestureRecognizerCollection.shared.GetAndroidBackButtonGestureRecognizer().OnGesture -= OnAndroidBackButton;
			gameObject.SetActive(false);
		}

		public void EditButtonClicked() { 
			renameDialog.Show(this);
		}

		public void LoadButtonClicked() {
			controllerDelegate.LoadButtonClicked(this);
		}

		public void ImportButtonClicked() {
			controllerDelegate.ImportButtonClicked(this);
			RefreshSearch();
			Refresh();
		}

		public void ExportButtonClicked() {
			controllerDelegate.ExportButtonClicked(this);
		}

		public void DeleteButtonClicked() {
			
			var filename = currentSelectionLabel.text;
			deleteConfirmation.ConfirmDeletionFor(filename, delegate(string name) {

				controllerDelegate.DeleteButtonClicked(this);
				RefreshSearch();
				if (this.searchResult.indices.Length > 0) {
					controllerDelegate.DidSelectItem(this, this.searchResult.indices[0]);
				} else {
					controllerDelegate.DidSelectItem(this, 0);
				}
				Refresh();
			});
		}

		public void CancelButtonClicked() {
			var d = controllerDelegate;
			Close();
			d.DidClose(this);
		}

		private void DeleteAllItemViews() {
			foreach (var itemView in items) {
				Destroy(itemView.gameObject);
			}
			items.Clear();
		}

		private void OnAndroidBackButton(AndroidBackButtonGestureRecognizer recognizer) {
			if (InputRegistry.shared.MayHandle(InputType.AndroidBack, this))
				Close();
		}

		// MARK: - SelectableListItemViewDelegate

		public void DidSelect(SelectableListItemView itemView) {
			if (controllerDelegate != null) {
				var index = items.IndexOf(itemView);
				if (index != -1) {
					DidSelectItemAtIndex(index);
					Refresh();
				}
			}
		}

		// MARK: - RenameDialogDelegate

		public void DidConfirmRename(RenameDialog dialog, string newName) {
			if (controllerDelegate != null) {
				controllerDelegate.DidEditTitleAtIndex(
					this, 
					controllerDelegate.GetIndexOfSelectedItem(this), 
					newName
				);
			}
			RefreshSearch();
			Refresh();
		}

		public bool CanEnterCharacter(RenameDialog dialog, int index, char c) {
			if (controllerDelegate != null) {
				return controllerDelegate.IsCharacterValidForName(this, c);
			}
			return false;
		}

		public void DidChangeValue(RenameDialog dialog, string value) {
			if (controllerDelegate != null) {
				int selectedIndex = controllerDelegate.GetIndexOfSelectedItem(this);
				string originalValue = controllerDelegate.GetTitleForItemAtIndex(this, selectedIndex);
				if (!controllerDelegate.IsNameAvailable(this, value) && value != originalValue) {
					dialog.ShowErrorMessage("This name is already used.");
				} else {
					dialog.ResetErrors();
				}
			}
		}

		public string GetOriginalName(RenameDialog dialog) {
			return currentSelectionLabel.text;
		}
	}
}