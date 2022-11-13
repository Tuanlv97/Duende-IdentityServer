using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TeduMicroservices.IDP.Infrastructure.Entities;
using TeduMicroservices.IDP.Infrastructure.Persistence;

namespace TeduMicroservices.IDP.Extensions
{
    public class IdentityUserStore : UserStore<User, IdentityRole, TeduIdentityContext>
    {
        public IdentityUserStore(TeduIdentityContext context, IdentityErrorDescriber describer = null) : base(context, describer)
        {
        }
        public override async Task<IList<string>> GetRolesAsync(User user, CancellationToken cancellationToken = default)
        {
            var query = from userRole in Context.UserRoles
                        join role in Context.Roles on userRole.RoleId equals role.Id
                        where userRole.UserId.Equals(user.Id)
                        select role.Id; // select role Id
            return await query.ToListAsync(cancellationToken); ;
        }
    }
}
