// ****************************************************************
// Copyright 2007, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org
// ****************************************************************

using System;

namespace NUnit.Util
{
	/// <summary>
	/// Summary description for RecentFilesService.
	/// </summary>
	public class RecentFilesService : RecentFiles, NUnit.Core.IService
	{
		private RecentFilesCollection fileEntries = new RecentFilesCollection();

		private ISettings settings;

		public static readonly int MinSize = 0;

		public static readonly int MaxSize = 24;

		public static readonly int DefaultSize = 5;

		#region Constructor
		public RecentFilesService()
			: this( Services.UserSettings ) { }

		public RecentFilesService( ISettings settings ) 
		{
			this.settings = settings;
		}
		#endregion

		#region Properties
		public int Count
		{
			get { return fileEntries.Count; }
		}

		public int MaxFiles
		{
			get 
			{ 
				int size = settings.GetSetting("Gui.RecentProjects.MaxFiles", -1 );
                if (size < 0)
                {
                    size = settings.GetSetting("RecentProjects.MaxFiles", DefaultSize);
                    if (size != DefaultSize)
                        settings.SaveSetting("Gui.RecentProjects.MaxFiles", size);
                    settings.RemoveSetting("RecentProjects.MaxFiles");
                }
				
				if ( size < MinSize ) size = MinSize;
				if ( size > MaxSize ) size = MaxSize;
				
				return size;
			}
			set 
			{ 
				int oldSize = MaxFiles;
				int newSize = value;
				
				if ( newSize < MinSize ) newSize = MinSize;
				if ( newSize > MaxSize ) newSize = MaxSize;

				settings.SaveSetting( "Gui.RecentProjects.MaxFiles", newSize );
				if ( newSize < oldSize ) SaveEntriesToSettings( this. settings );
			}
		}
		#endregion

		#region Public Methods
		public RecentFilesCollection Entries
		{
			get
			{
				return fileEntries;
			}
		}
		
		public void Remove( string fileName )
		{
			fileEntries.Remove( fileName );
		}

		public void SetMostRecent( string fileName )
		{
			SetMostRecent( new RecentFileEntry( fileName ) );
		}

		public void SetMostRecent( RecentFileEntry entry )
		{
			int index = fileEntries.IndexOf(entry.Path);

			if(index != -1)
				fileEntries.RemoveAt(index);

			fileEntries.Insert( 0, entry );
			if( fileEntries.Count > MaxFiles )
				fileEntries.RemoveAt( MaxFiles );
		}
		#endregion

		#region Helper Methods for saving and restoring the settings
		private void LoadEntriesFromSettings( ISettings settings )
		{
			fileEntries.Clear();

            AddEntriesForPrefix("Gui.RecentProjects");

            // Try legacy entries if nothing was found
            if (fileEntries.Count == 0)
            {
                AddEntriesForPrefix("RecentProjects.V2");
                AddEntriesForPrefix("RecentProjects.V1");
            }

            // Try even older legacy format
            if (fileEntries.Count == 0)
                AddEntriesForPrefix("RecentProjects");
		}

        private void AddEntriesForPrefix(string prefix)
        {
            for (int index = 1; index < MaxFiles; index++)
            {
                if (fileEntries.Count >= MaxFiles) break;

                string fileSpec = settings.GetSetting(GetRecentFileKey(prefix, index)) as string;
                if (fileSpec != null) fileEntries.Add(RecentFileEntry.Parse(fileSpec));
            }
        }

		private void SaveEntriesToSettings( ISettings settings )
		{
			string prefix = "Gui.RecentProjects";

			while( fileEntries.Count > MaxFiles )
				fileEntries.RemoveAt( fileEntries.Count - 1 );

			for( int index = 0; index < MaxSize; index++ ) 
			{
				string keyName = GetRecentFileKey( prefix, index + 1 );
				if ( index < fileEntries.Count )
					settings.SaveSetting( keyName, fileEntries[index].Path );
				else
					settings.RemoveSetting( keyName );
			}

            // Remove legacy entries here
            settings.RemoveGroup("RecentProjects");
		}

		private string GetRecentFileKey( string prefix, int index )
		{
			return string.Format( "{0}.File{1}", prefix, index );
		}
		#endregion

		#region IService Members

		public void UnloadService()
		{
			SaveEntriesToSettings( this.settings );
		}

		public void InitializeService()
		{
			LoadEntriesFromSettings( this.settings );
		}

		#endregion
	}
}
