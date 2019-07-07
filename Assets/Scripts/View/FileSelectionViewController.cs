using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Keiwando.UI;
using Keiwando;

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

    public void Show(FileSelectionViewControllerDelegate Delegate) {
        this.controllerDelegate = Delegate;

		InputRegistry.shared.Register(InputType.All, delegate(InputType type) {});

		DeleteAllItemViews();
		gameObject.SetActive(true);
		Refresh();
    }

	public void Refresh() {
		if (controllerDelegate == null) { return; }

		titleLabel.text = controllerDelegate.GetTitle(this);
		RefreshItemList();
		RefreshCurrentSelection();

		if (items.Count == 0) {
			emptyMessageLabel.text = controllerDelegate.GetEmptyMessage(this);
		}
		emptyMessageLabel.gameObject.SetActive(items.Count == 0);
	}

	private void RefreshItemList() {

		var container = itemTemplate.transform.parent;
		// Create all necessary item views first
		int diff = controllerDelegate.GetNumberOfItems(this) - items.Count;

		if (diff < 0) {
			// Delete unneeded item views
			var toDelete = items.GetRange(items.Count + diff - 1, -diff);
			items.RemoveRange(items.Count + diff - 1, -diff);
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

		var containerRect = container.GetComponent<RectTransform>();
		var size = containerRect.sizeDelta;
		size = new Vector2(size.x, items.Count * (itemList.CellHeight + itemList.Spacing));
		containerRect.sizeDelta = size;

		// Update item views
		int selectedIndex = controllerDelegate.GetIndexOfSelectedItem(this);
		for (int i = 0; i < items.Count; i++) {

			string title = controllerDelegate.GetTitleForItemAtIndex(this, i);
			items[i].Refresh(title, selectedIndex == i);
		}
	}

	private void RefreshCurrentSelection() {
		int selectedIndex = controllerDelegate.GetIndexOfSelectedItem(this);
		string title = controllerDelegate.GetTitleForItemAtIndex(this, selectedIndex);
		currentSelectionLabel.text = title;
	}

    public void Close() {
		controllerDelegate = null;
		InputRegistry.shared.Deregister();
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
		Refresh();
    }

    public void ExportButtonClicked() {
		controllerDelegate.ExportButtonClicked(this);
    }

    public void DeleteButtonClicked() {
		
		var filename = currentSelectionLabel.text;
		deleteConfirmation.ConfirmDeletionFor(filename, delegate(string name) {

			controllerDelegate.DeleteButtonClicked(this);
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

	// MARK: - SelectableListItemViewDelegate

	public void DidSelect(SelectableListItemView itemView) {
		if (controllerDelegate != null) {
			var index = items.IndexOf(itemView);
			if (index != -1) {
				controllerDelegate.DidSelectItem(this, index);
				Refresh();
			}
		}
	}

	// MARK: - RenameDialogDelegate

	public void DidConfirmRename(RenameDialog dialog, string newName) {
		if (controllerDelegate != null) {
			controllerDelegate.DidEditTitleAtIndex(this, 
												   controllerDelegate.GetIndexOfSelectedItem(this), 
												   newName);
		}
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