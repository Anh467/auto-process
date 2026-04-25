using System;
using System.Collections.Generic;
using System.Text;

namespace AutoProcess.Domain.Entities.Base
{
    public interface IAudited
    {
        /// <summary>
        /// <para>The date and time this entity was created</para>
        /// <para>Does not need to be explicitly set when calling <c>Save</c></para>
        /// </summary>
        DateTimeOffset CreatedAt { get; set; }

        /// <summary>
        /// <para>The date and time this entity was last updated</para>
        /// <para>Does not need to be explicitly set when calling <c>Save</c></para>
        /// </summary>
        DateTimeOffset UpdatedAt { get; set; }

        /// <summary>
        /// <para>The date and time this entity was deleted</para>
        /// <para>Does not need to be explicitly set when calling <c>SoftDelete</c></para>
        /// </summary>
        DateTimeOffset? DeletedAt { get; set; }
    }
}
