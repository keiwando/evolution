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
    public string filePath;
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

    // DEBUG: Remove when done prototyping
    public static List<CreatureGalleryEntry> dbg_entries = new List<CreatureGalleryEntry>();

    public CreatureGallery gallery = new CreatureGallery();

    public void loadGalleryEntries() {

      gallery.entries.AddRange(dbg_entries);
    }
  }
}
