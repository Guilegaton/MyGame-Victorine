namespace MGV.Models
{
    public class File
    {
        #region Public Properties

        public byte[] AsBytes { get; set; }
        public string Extention { get; set; }
        public string FileType { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }

        #endregion Public Properties
    }
}