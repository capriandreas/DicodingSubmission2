using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BlobHelper
{
    public class BlobReference
    {
        public string BlobUri { get; set; }

        public Stream BlobStream { get; set; }
    }
}
