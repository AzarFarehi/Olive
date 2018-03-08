﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Olive.Entities.Attributes
{
    /// <summary>
    /// When applied to a class field or property, it marks it as the default string representation of that class.
    /// This is intended to be used by Code generators when generating a ToString() method, to know what to return.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ToStringAttribute : Attribute { }
}