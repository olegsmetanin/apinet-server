using System;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Mapping;
using AGO.Core.Attributes.Model;
using AGO.Core.Controllers.Security.OAuth;
using Newtonsoft.Json;

namespace AGO.Core.Model.Security
{
	public class UserModel : CoreModel<Guid>
	{
		private const int EMAIL_SIZE_CONST = 64;
		private const int NAME_SIZE = 64;
		public static readonly int EMAIL_SIZE = EMAIL_SIZE_CONST;//because const will be inlined in using classes, but not const is not compile time value
		public const int FULLNAME_SIZE = NAME_SIZE * 2 + 1;

		#region Persistent

		private string lastName;
		private string firstName;

		[JsonProperty, NotLonger(EMAIL_SIZE_CONST)]
		public virtual string Email { get; set; }

		[JsonProperty, NotNull]
		public virtual bool Active { get; set; }

		[JsonProperty, NotLonger(NAME_SIZE), NotEmpty]
		public virtual string FirstName
		{
			get { return firstName; }
			set
			{
				if (firstName == value) return;

				firstName = value;
				CalculateNames();
			}
		}

		[JsonProperty, NotLonger(NAME_SIZE)]
		public virtual string LastName
		{
			get { return lastName; }
			set
			{
				if (lastName == value) return;

				lastName = value;
				CalculateNames();
			}
		}

		[JsonProperty, NotLonger(FULLNAME_SIZE)]
		public virtual string FullName { get; protected internal set; }

		[JsonProperty, NotEmpty]
		public virtual SystemRole SystemRole { get; set; }
		
		[NotLonger(1024), JsonProperty]
		public virtual string AvatarUrl { get; set; }

		[MetadataExclude]
		public virtual OAuthProvider? OAuthProvider { get; set; }

		[MetadataExclude, NotLonger(EMAIL_SIZE_CONST)]
		public virtual string OAuthUserId { get; set; }

		#endregion

		private void CalculateNames()
		{
			FullName = FirstName.IsNullOrWhiteSpace()
				? LastName.IsNullOrWhiteSpace() ? string.Empty : LastName.TrimSafe()
				: LastName.IsNullOrWhiteSpace() 
					? FirstName.TrimSafe() 
					: FirstName.TrimSafe() + " " + LastName.TrimSafe();
		}

		[NotMapped, MetadataExclude]
		public virtual bool IsAdmin
		{
			get { return SystemRole == SystemRole.Administrator; }
		}

		public override string ToString()
		{
			return (FullName.IsNullOrWhiteSpace() ? Email : FullName).TrimSafe();
		}
	}
}
