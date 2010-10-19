using System;
using System.IO;
namespace AutoTest.Messages
{
	public class ChangedFile
	{
		private readonly string _fullName;
        private readonly string _name;
        private readonly string _extension;

        public string Extension { get { return _extension; } }
        public string FullName { get { return _fullName; } }
        public string Name { get { return _name; } }
		
		public ChangedFile()
		{
			_fullName = "";
			_name = "";
			_extension = "";
		}

        public ChangedFile(string fullFilePath)
        {
            if (fullFilePath == null)
                throw new ArgumentNullException("File path cannot be null");
            _fullName = fullFilePath;
            _name = Path.GetFileName(_fullName);
            _extension = Path.GetExtension(_name);
        }
	}
}
