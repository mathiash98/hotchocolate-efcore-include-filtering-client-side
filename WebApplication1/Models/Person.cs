namespace WebApplication1.Models
{
    using HotChocolate.Data;
    using HotChocolate.Types;

    public class Person : ConfigEntityBaseClass
    {
        public string? FullName { get; set; }

        [UsePaging]
        [UseFiltering]
        [UseSorting]
        public List<PersonAction> Actions { get; set; } = new List<PersonAction>();
        
        public string? BoardingState { get; set; }

        public override void AddSoftDeleteActionForEntity(AppContext sql)
        {
            //Make sure all person actions are also softdeleted
            var nonDeletedActions = sql.PersonActions.Where(a => a.Person.Id == Id && a.DeletedUtcDateTime == null).ToList();
            foreach (var action in nonDeletedActions)
            {
                action.SoftDelete(sql);
            }
        }
    }
}
