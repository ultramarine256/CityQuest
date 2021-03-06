﻿using Abp.Domain.Entities.Auditing;
using CityQuest.Entities.GameModule.Keys;
using CityQuest.Entities.GameModule.Locations;
using CityQuest.Entities.GameModule.PlayerCareers;
using CityQuest.Entities.GameModule.Teams.TeamRequests;
using CityQuest.Entities.MainModule.Authorization.UserLogins;
using CityQuest.Entities.MainModule.Authorization.UserRoles;
using CityQuest.Entities.MainModule.Authorization.UserServices;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CityQuest.Entities.MainModule.Users
{
    public class User : FullAuditedEntity<long, User>, IUser<long>
    {
        #region Ctors

        public User()
        {
            ActivatedKeys = new HashSet<Key>();
            PlayerCareers = new HashSet<PlayerCareer>();
            TeamRequests = new HashSet<TeamRequest>();
            Logins = new HashSet<UserLogin>();
            Roles = new HashSet<UserRole>();
            Permissions = new HashSet<UserPermissionSetting>();
        }

        #endregion

        #region Relations

        public long? LocationId { get; set; }
        public Location Location { get; set; }

        public virtual ICollection<Key> ActivatedKeys { get; set; }
        public virtual ICollection<PlayerCareer> PlayerCareers { get; set; }
        public virtual ICollection<TeamRequest> TeamRequests { get; set; }

        public virtual ICollection<UserLogin> Logins { get; set; }
        public virtual ICollection<UserRole> Roles { get; set; }
        public virtual ICollection<UserPermissionSetting> Permissions { get; set; }

        #endregion

        public string Name { get; set; }
        public string Surname { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Is the <see cref="EmailAddress"/> confirmed.
        /// </summary>
        public bool IsEmailConfirmed { get; set; }
        /// <summary>
        /// Confirmation code for email.
        /// </summary>
        public string EmailConfirmationCode { get; set; }
        /// <summary>
        /// The last time this user entered to the system.
        /// </summary>
        public virtual DateTime? LastLoginTime { get; set; }

        public string FullName 
        {
            get
            {
                return string.Format("{0} {1}", this.Name, this.Surname);
            }
        }

        public string FullUserName 
        {
            get
            {
                return string.Format("{0} ({1} {2})", this.UserName, this.Name, this.Surname);
            }
        }

        public PlayerCareer CurrentPlayerCareer 
        {
            get 
            {
                return GetPlayerCareer(DateTime.Now);
            }
        }

        public long? CurrentPlayerTeamId
        {
            get
            {
                return GetPlayerTeamId(DateTime.Now);
            }
        }

        public PlayerCareer GetPlayerCareer(DateTime dateTime)
        {
            return PlayerCareers
                .Where(r => r.CareerDateStart <= dateTime && (r.CareerDateEnd == null || r.CareerDateEnd >= dateTime))
                .SingleOrDefault();
        }

        public long? GetPlayerTeamId(DateTime dateTime)
        {
            PlayerCareer playerCareer = GetPlayerCareer(dateTime);

            return playerCareer != null ? (long?)playerCareer.TeamId : null;
        }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager manager)
        {
            // authenticationType have match with CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            return userIdentity;
        }
    }
}
