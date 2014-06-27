namespace SimpleBrowser
{
	using System;
	using System.Text;

	/// <summary>
	/// A class for basic authentication credentials.
	/// </summary>
	public class BasicAuthenticationToken
	{
		/// <summary>
		/// Gets the basic authentication token.
		/// </summary>
		public string Token { get; private set; }

		/// <summary>
		/// Gets the basic authentication expiration date and time.
		/// </summary>
		public DateTime Expiration { get; private set; }

		/// <summary>
		/// Gets the domain associated with the token.
		/// </summary>
		public string Domain { get; private set; }

		/// <summary>
		/// Gets or sets the number of minutes before an inactive token expires.
		/// </summary>
		private static uint _timeout = 15;
		public static uint Timeout
		{
			get
			{
				return _timeout;
			}

			set
			{
				int timeout = BasicAuthenticationToken.Timeout > int.MaxValue ? int.MaxValue : (int)value;
				_timeout = (uint)timeout;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="BasicAuthenticationToken"/> class.
		/// </summary>
		/// <param name="domain">The domain of the site being autheticated</param>
		/// <param name="username">The user name of the user</param>
		/// <param name="password">The password of the user</param>
		public BasicAuthenticationToken(string domain, string username, string password)
		{
			if (string.IsNullOrWhiteSpace(domain))
			{
				throw new ArgumentNullException("domain");
			}

			if (string.IsNullOrWhiteSpace(username))
			{
				throw new ArgumentNullException("username");
			}

			if (string.IsNullOrWhiteSpace(password))
			{
				throw new ArgumentNullException("password");
			}

			this.Domain = domain;
			this.Token = Convert.ToBase64String(Encoding.UTF8.GetBytes(username + ":" + password));
			this.UpdateExpiration();
		}

		/// <summary>
		/// Refreshes the expiration date
		/// </summary>
		/// <remarks>
		/// For example, when created, the token expires after X minutes. Each time the token is used,
		/// the expiration is updates to X minutes from the last time it was used. That way, If a user
		/// is active on a site for 30 minutes, he/she will not be logged out after the default 15 minutes.
		/// This method is called to update the token after it has been used.
		/// </remarks>
		public void UpdateExpiration()
		{
			this.Expiration = DateTime.Now + new TimeSpan(0, (int)BasicAuthenticationToken.Timeout, 0);
		}
	}
}
