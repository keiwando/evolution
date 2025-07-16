using System;
using UnityEngine;
using System.Collections.Generic;

namespace Keiwando.Evolution {

  public class LoadedCreatureGalleryEntry {
    public CreatureRecording recording;

    public LoadedCreatureGalleryEntry(CreatureRecording recording) {
      this.recording = recording;
    }
  }

  public struct CreatureGalleryEntry {
    public string filename;
    public DateTime createdDate;
    public LoadedCreatureGalleryEntry loadedData;
  }

  public class CreatureGallery {

    public List<CreatureGalleryEntry> entries;

    public CreatureGallery() {
      this.entries = new List<CreatureGalleryEntry>();
    }
  }

  public class CreatureGalleryManager {

    public CreatureGallery gallery = new CreatureGallery();

    public void shallowLoadGalleryEntries() {
      gallery.entries.Clear();
      
      List<string> filenames = CreatureRecordingSerializer.GetCreatureRecordingFilenames();

      foreach (string filename in filenames) {
        string filenameWithoutExtension = CreatureRecordingSerializer.EXTENSION_PATTERN.Replace(filename, "");
        DateTime? creationDate = CreatureRecordingSerializer.LoadCreatureRecordingDate(filenameWithoutExtension);
        if (creationDate != null) {
          gallery.entries.Add(new CreatureGalleryEntry {
            filename = filenameWithoutExtension,
            createdDate = creationDate.Value,
            loadedData = null
          });
        }
      }
    }

    public void loadGalleryEntry(int index) {
      if (index < 0 || index >= gallery.entries.Count) {
        Debug.LogError($"Invalid gallery entry load index {index}");
        return;
      }

      CreatureGalleryEntry galleryEntry = gallery.entries[index];
      if (galleryEntry.loadedData != null) {
        // Data is already loaded
        return;
      }

      CreatureRecording recording = CreatureRecordingSerializer.LoadCreatureRecording(galleryEntry.filename);
      if (recording == null) {
        Debug.Log($"Failed to deserialize creature recording from file {galleryEntry.filename}");
        return;
      }
      galleryEntry.loadedData = new LoadedCreatureGalleryEntry(recording);

      gallery.entries[index] = galleryEntry;
    }

    public void unloadGalleryEntry(int index) {
      if (index < 0 || index >= gallery.entries.Count) {
        Debug.LogError($"Invalid gallery entry unload index {index}");
        return;
      }

      CreatureGalleryEntry galleryEntry = gallery.entries[index];
      if (galleryEntry.loadedData == null) {
        // Data is not loaded
        return;
      }

      galleryEntry.loadedData = null; // Unload the data
      gallery.entries[index] = galleryEntry;
    }

    public void unloadAllGalleryEntries() {
      for (int i = 0; i < gallery.entries.Count; i++) {
        CreatureGalleryEntry galleryEntry = gallery.entries[i];
        if (galleryEntry.loadedData != null) {
          galleryEntry.loadedData = null; // Unload the data
          gallery.entries[i] = galleryEntry;
        }
      }
    }

    public void deleteGalleryEntry(CreatureGalleryEntry entry) {
      var path = CreatureRecordingSerializer.PathToCreatureRecordingSave(entry.filename);
    }
  }
}
