// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Azure.Messaging.ServiceBus.Administration
{
    /// <summary>
    /// Specifies the authorization rules.
    /// </summary>
    public class AuthorizationRules : List<AuthorizationRule>, IEquatable<AuthorizationRules>
    {
        /// <summary>
        /// Creates an XML representation of the AuthorizationRules.
        /// </summary>
        /// <returns>The XElement that represents the AuthorizationRules.</returns>
        internal XElement Serialize()
        {
            var rules = new XElement(
                XName.Get("AuthorizationRules", AdministrationClientConstants.ServiceBusNamespace),
                this.Select(rule => rule.Serialize()));

            return rules;
        }

        /// <summary>
        /// Creates a clone of AuthorizationRules.
        /// </summary>
        /// <returns>The cloned AuthorizationRules.</returns>
        internal AuthorizationRules Clone()
        {
            var rules = new AuthorizationRules();
            foreach (AuthorizationRule rule in this)
            {
                rules.Add(rule.Clone());
            }
            return rules;
        }

        /// <summary>
        /// The constructor for AuthorizationRules for internal use only.
        /// </summary>
        internal AuthorizationRules() { }

        /// <summary>
        /// Creates an AuthorizationRules based on an XML representation.
        /// </summary>
        /// <param name="xElement">The XElement that represents an AuthorizationRules.</param>
        /// <returns>The AuthorizationRules created from the XElement.</returns>
        internal static AuthorizationRules ParseFromXElement(XElement xElement)
        {
            var rules = new AuthorizationRules();
            var xRules = xElement.Elements(XName.Get("AuthorizationRule", AdministrationClientConstants.ServiceBusNamespace));
            rules.AddRange(xRules.Select(rule => AuthorizationRule.ParseFromXElement(rule)));
            return rules;
        }

        /// <summary>
        ///   Returns a hash code for this instance.
        /// </summary>
        public override int GetHashCode()
        {
            int hash = 7;
            unchecked
            {
                foreach (AuthorizationRule rule in this)
                {
                    hash = (hash * 7) + rule.GetHashCode();
                }
            }

            return hash;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        public override bool Equals(object obj)
        {
            var other = obj as AuthorizationRules;
            return Equals(other);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        public bool Equals(AuthorizationRules other)
        {
            if (other is null || Count != other.Count)
            {
                return false;
            }

            var cnt = new Dictionary<string, AuthorizationRule>();
            foreach (AuthorizationRule rule in this)
            {
                cnt[rule.KeyName] = rule;
            }

            foreach (AuthorizationRule otherRule in other)
            {
                if (!cnt.TryGetValue(otherRule.KeyName, out var rule) || !rule.Equals(otherRule))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>Compares two AuthorizationRules to each other for equality.</summary>
        public static bool operator ==(AuthorizationRules left, AuthorizationRules right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (left is null || right is null)
            {
                return false;
            }

            return left.Equals(right);
        }

        /// <summary>Compares two AuthorizationRules to each other for inequality.</summary>
        public static bool operator !=(AuthorizationRules left, AuthorizationRules right)
        {
            return !(left == right);
        }
    }
}
