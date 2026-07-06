using Domain.Common;

namespace Domain.Entities
{
    public class FileAttachment : SoftDeleteEntity
    {
        public string FileName { get; set; } = string.Empty;

        public string OriginalFileName { get; set; } = string.Empty;

        public string FilePath { get; set; } = string.Empty;

        public string Extension { get; set; } = string.Empty;

        public string ContentType { get; set; } = string.Empty;

        public long Size { get; set; }

        public Guid UploadedByUserId { get; set; }

        public string Module { get; set; } = string.Empty;

        public Guid ReferenceId { get; set; }

        public bool IsPublic { get; set; }

        public virtual User UploadedByUser { get; set; } = null!;
    }
}
