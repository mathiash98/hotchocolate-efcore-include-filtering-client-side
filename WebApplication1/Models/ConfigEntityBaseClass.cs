using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models;

public abstract class ConfigEntityBaseClass
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public DateTime? DeletedUtcDateTime { get; set; }

        public ConfigEntityBaseClass()
        {
            LastUpdatedAt = DateTime.UtcNow;
        }
        public ConfigEntityBaseClass(Guid id, DateTime lastUpdatedAt, DateTime? deletedUtcDateTime)
        {
            Id = id;
            LastUpdatedAt = lastUpdatedAt;
            DeletedUtcDateTime = deletedUtcDateTime;
        }

        public void UpdateLastUpdatedAt()
        {
            LastUpdatedAt = DateTime.UtcNow;
        }

        public void Update(DateTime lastUpdatedAt, DateTime? deletedUtcDateTime, AppContext sql)
        {
            LastUpdatedAt = lastUpdatedAt;
            DeletedUtcDateTime = deletedUtcDateTime;
            if (DeletedUtcDateTime != null)
            {
                AddSoftDeleteActionForEntity(sql);
            }
        }

        public void SoftDelete(AppContext sql)
        {
            DeletedUtcDateTime = LastUpdatedAt = DateTime.UtcNow;
            AddSoftDeleteActionForEntity(sql);
        }
        public virtual void ValidateThatSiteIsCorrectForUpdate(Guid siteId)
        {
            throw new NotImplementedException();
        }

        public virtual List<string> GetQueryIncludeEntities()
        {
            return null;
        }
        public virtual void AddSoftDeleteActionForEntity(AppContext sql) { }
    }