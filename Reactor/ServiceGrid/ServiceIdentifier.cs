using System;

namespace Reactor.ServiceGrid
{
    public class ServiceIdentifier : IEquatable<ServiceIdentifier>
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceIdentifier"/> class.
        /// </summary>
        public ServiceIdentifier() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceIdentifier"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="version">The version.</param>
        public ServiceIdentifier(string name, string version) : this(name, new Version(version)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceIdentifier"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="version">The version.</param>
        public ServiceIdentifier(string name, Version version)
        {
            Name = name;
            Version = version;
        }

        #endregion

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        /// <value>The version.</value>
        public Version Version { get; set; }

        #region Equality Members

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(ServiceIdentifier other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Name, Name) && Equals(other.Version, Version);
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
        /// </returns>
        /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>. </param><filterpriority>2</filterpriority>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(ServiceIdentifier)) return false;
            return Equals((ServiceIdentifier)obj);
        }

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            unchecked
            {
                return ((Name != null ? Name.GetHashCode() : 0) * 397) ^ (Version != null ? Version.GetHashCode() : 0);
            }
        }

        public static bool operator ==(ServiceIdentifier left, ServiceIdentifier right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ServiceIdentifier left, ServiceIdentifier right)
        {
            return !Equals(left, right);
        }

        #endregion

        public static bool TryParseFromDirectoryName(string directoryName, out ServiceIdentifier serviceIdentifier)
        {
            serviceIdentifier = null;

            if (!directoryName.Contains("-"))
            {
                return false;
            }

            var parts = directoryName.Split('-');
            if (parts.Length != 2)
            {
                return false;
            }

            // Validate version
            Version version;
            if (!Version.TryParse(parts[1], out version))
            {
                return false;
            }

            serviceIdentifier = new ServiceIdentifier
            {
                Name = parts[0],
                Version = version
            };

            return true;
        }

        public override string ToString()
        {
            return string.Format("{0}-{1}", Name, Version);
        }

        public string ToServiceInstanceString()
        {
            return string.Format("{0}$v{1}", Name, Version);
        }

        public static bool TryParseFromServiceInstanceString(string serviceInstanceString, out ServiceIdentifier serviceIdentifier)
        {
            if (string.IsNullOrEmpty(serviceInstanceString)) throw new ArgumentNullException("serviceInstanceString");

            serviceIdentifier = null;

            if (!serviceInstanceString.Contains("$v")) return false;

            serviceInstanceString = serviceInstanceString.Replace("$v", "|");
            var parts = serviceInstanceString.Split('|');
            if (parts.Length != 2) return false;

            serviceIdentifier = new ServiceIdentifier
            {
                Name = parts[0],
                Version = new Version(parts[1])
            };
            return true;
        }
    }
}
