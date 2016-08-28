using System;
using System.Collections.Generic;
using System.Security.Permissions;

namespace WrapSharp {
    class BoundDirectory {
        public string DirPath { get; private set; }
        public IList<FileIOPermissionAccess> DirPermissions { get; private set; }

        public BoundDirectory(string optionEntry) {
            DirPath = optionEntry;
            DirPermissions = new List<FileIOPermissionAccess>();

            DirPermissions.Add(FileIOPermissionAccess.AllAccess);
        }
    }
}
