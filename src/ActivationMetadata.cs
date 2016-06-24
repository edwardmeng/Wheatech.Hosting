﻿using System.Reflection;

namespace Wheatech.Activation
{
    internal class ActivationMetadata
    {
        private ActivationPriority? _priority;
        private readonly ActivationPriority _defaultPriority;

        public ActivationMetadata(MemberInfo targetMember, ActivationPriority defaultPriority = ActivationPriority.Normal)
        {
            TargetMember = targetMember;
            _defaultPriority = defaultPriority;
        }

        public MemberInfo TargetMember { get; }

        public object TargetInstance { get; set; }

        public ActivationPriority Priority
        {
            get
            {
                if (!_priority.HasValue)
                {
                    var attribute = TargetMember.GetCustomAttribute<ActivationPriorityAttribute>();
                    _priority = attribute?.Priority ?? _defaultPriority;
                }
                return _priority.Value;
            }
        }
    }
}