using DynamicData;
using System.Reactive.Linq;

namespace Ty.Services
{
    public class PermissionService
    {
        private SourceCache<string, string> Permissions { get; set; } = new SourceCache<string, string>(c => c);


        public void AddPermission(string permission)
        {
            Permissions.AddOrUpdate(permission);
        }

        public void RemovePermission(string permission)
        {
            Permissions.RemoveKey(permission);
        }

        /// <summary>
        /// HasPermission("permissionName").ToProperty(this, x => x.Admin, out admin);
        /// </summary>
        /// <param name="permission"></param>
        /// <returns></returns>
        public IObservable<bool> HasPermission(params string[] permissions)
        {
            var observables = permissions.Select(permission => Permissions.Watch(permission).Select(c => c.Reason != ChangeReason.Remove));

            return observables.CombineLatest().Select(values => values.All(value => value));

        }
    }
}
