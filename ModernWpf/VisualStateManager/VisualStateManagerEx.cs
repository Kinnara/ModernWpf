// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ModernWpf
{
    /// <summary>
    ///     VisualStateManager with WinUI-style VisualStateEx.Setters support.
    /// </summary>
    public class VisualStateManagerEx : SimpleVisualStateManager
    {
        protected override bool GoToStateCore(
            FrameworkElement control,
            FrameworkElement stateGroupsRoot,
            string stateName,
            VisualStateGroup group,
            VisualState state,
            bool useTransitions)
        {
            if (state == null)
            {
                return false;
            }

            var resolvedSetters = ResolveSetters(control, stateGroupsRoot, state);
            bool result = base.GoToStateCore(control, stateGroupsRoot, stateName, group, state, useTransitions);

            if (result && group != null)
            {
                GetSetterValueStore(stateGroupsRoot).ChangeState(group, resolvedSetters);
            }

            return result;
        }

        private static IReadOnlyList<ResolvedVisualStateSetter> ResolveSetters(
            FrameworkElement control,
            FrameworkElement stateGroupsRoot,
            VisualState state)
        {
            if (!(state is VisualStateEx stateEx) || stateEx.Setters.Count == 0)
            {
                return Array.Empty<ResolvedVisualStateSetter>();
            }

            var resolvedSetters = new List<ResolvedVisualStateSetter>(stateEx.Setters.Count);
            foreach (VisualStateSetter setter in stateEx.Setters)
            {
                resolvedSetters.Add(ResolveSetter(control, stateGroupsRoot, setter));
            }

            return resolvedSetters;
        }

        private static ResolvedVisualStateSetter ResolveSetter(
            FrameworkElement control,
            FrameworkElement stateGroupsRoot,
            VisualStateSetter setter)
        {
            ParsedTargetPath targetPath = ParsedTargetPath.Parse(setter);
            DependencyObject target = ResolveTarget(control, stateGroupsRoot, targetPath.TargetName);
            DependencyProperty property = ResolveDependencyProperty(target, targetPath.PropertyPath);
            object value = ConvertValue(setter.Value, property);

            return new ResolvedVisualStateSetter(target, property, value);
        }

        private static DependencyObject ResolveTarget(FrameworkElement control, FrameworkElement stateGroupsRoot, string targetName)
        {
            if (string.IsNullOrWhiteSpace(targetName))
            {
                return control ?? stateGroupsRoot;
            }

            object target = null;

            if (stateGroupsRoot != null)
            {
                if (string.Equals(stateGroupsRoot.Name, targetName, StringComparison.Ordinal))
                {
                    target = stateGroupsRoot;
                }
                else
                {
                    target = stateGroupsRoot.FindName(targetName);
                }
            }

            if (target == null && control != null)
            {
                if (string.Equals(control.Name, targetName, StringComparison.Ordinal))
                {
                    target = control;
                }
                else
                {
                    target = control.FindName(targetName);
                }

                if (target == null && control is Control controlWithTemplate && controlWithTemplate.Template != null)
                {
                    target = controlWithTemplate.Template.FindName(targetName, controlWithTemplate);
                }
            }

            if (target is DependencyObject dependencyObject)
            {
                return dependencyObject;
            }

            throw new InvalidOperationException($"Unable to resolve visual state setter target '{targetName}'.");
        }

        private static DependencyProperty ResolveDependencyProperty(DependencyObject target, string propertyPath)
        {
            if (string.IsNullOrWhiteSpace(propertyPath))
            {
                throw new InvalidOperationException("Visual state setter property path cannot be empty.");
            }

            if (TryParseAttachedPropertyPath(propertyPath, out string ownerTypeName, out string attachedPropertyName))
            {
                Type ownerType = ResolveOwnerType(ownerTypeName);
                return ResolveDependencyPropertyField(ownerType, attachedPropertyName, propertyPath);
            }

            if (propertyPath.IndexOf('.') >= 0 || propertyPath.IndexOf('(') >= 0 || propertyPath.IndexOf(')') >= 0)
            {
                throw new NotSupportedException($"Nested visual state setter target path '{propertyPath}' is not supported.");
            }

            return ResolveDependencyPropertyField(target.GetType(), propertyPath, propertyPath);
        }

        private static bool TryParseAttachedPropertyPath(string propertyPath, out string ownerTypeName, out string propertyName)
        {
            ownerTypeName = null;
            propertyName = null;

            if (!propertyPath.StartsWith("(", StringComparison.Ordinal) ||
                !propertyPath.EndsWith(")", StringComparison.Ordinal))
            {
                return false;
            }

            string body = propertyPath.Substring(1, propertyPath.Length - 2);
            int separatorIndex = body.LastIndexOf('.');
            if (separatorIndex <= 0 || separatorIndex == body.Length - 1)
            {
                throw new NotSupportedException($"Attached visual state setter target path '{propertyPath}' is invalid.");
            }

            ownerTypeName = body.Substring(0, separatorIndex);
            propertyName = body.Substring(separatorIndex + 1);
            return true;
        }

        private static DependencyProperty ResolveDependencyPropertyField(Type ownerType, string propertyName, string originalPath)
        {
            const BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy;
            FieldInfo field = ownerType.GetField(propertyName + "Property", Flags);

            if (field?.GetValue(null) is DependencyProperty dependencyProperty)
            {
                return dependencyProperty;
            }

            throw new InvalidOperationException($"Unable to resolve dependency property '{originalPath}' on '{ownerType.FullName}'.");
        }

        private static Type ResolveOwnerType(string ownerTypeName)
        {
            string normalizedName = StripXamlPrefix(ownerTypeName);

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = assembly.GetType(normalizedName, throwOnError: false);
                if (type != null)
                {
                    return type;
                }
            }

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = null;

                try
                {
                    type = assembly
                        .GetTypes()
                        .FirstOrDefault(candidate =>
                            string.Equals(candidate.Name, normalizedName, StringComparison.Ordinal) ||
                            string.Equals(candidate.FullName, normalizedName, StringComparison.Ordinal));
                }
                catch (ReflectionTypeLoadException ex)
                {
                    type = ex.Types
                        .Where(candidate => candidate != null)
                        .FirstOrDefault(candidate =>
                            string.Equals(candidate.Name, normalizedName, StringComparison.Ordinal) ||
                            string.Equals(candidate.FullName, normalizedName, StringComparison.Ordinal));
                }

                if (type != null)
                {
                    return type;
                }
            }

            throw new InvalidOperationException($"Unable to resolve attached-property owner type '{ownerTypeName}'.");
        }

        private static string StripXamlPrefix(string value)
        {
            int prefixIndex = value.IndexOf(':');
            return prefixIndex >= 0 ? value.Substring(prefixIndex + 1) : value;
        }

        private static object ConvertValue(object value, DependencyProperty property)
        {
            Type propertyType = property.PropertyType;

            if (value == null || value == DependencyProperty.UnsetValue || propertyType.IsInstanceOfType(value))
            {
                return value;
            }

            if (value is string stringValue)
            {
                TypeConverter converter = TypeDescriptor.GetConverter(propertyType);
                if (converter.CanConvertFrom(typeof(string)))
                {
                    return converter.ConvertFrom(null, CultureInfo.InvariantCulture, stringValue);
                }
            }

            TypeConverter valueConverter = TypeDescriptor.GetConverter(value.GetType());
            if (valueConverter.CanConvertTo(propertyType))
            {
                return valueConverter.ConvertTo(null, CultureInfo.InvariantCulture, value, propertyType);
            }

            throw new InvalidOperationException(
                $"Unable to convert visual state setter value '{value}' to '{propertyType.FullName}' for '{property.Name}'.");
        }

        private static SetterValueStore GetSetterValueStore(FrameworkElement stateGroupsRoot)
        {
            var store = (SetterValueStore)stateGroupsRoot.GetValue(SetterValueStoreProperty);
            if (store == null)
            {
                store = new SetterValueStore();
                stateGroupsRoot.SetValue(SetterValueStoreProperty, store);
            }

            return store;
        }

        private static readonly DependencyProperty SetterValueStoreProperty =
            DependencyProperty.RegisterAttached(
                "SetterValueStore",
                typeof(SetterValueStore),
                typeof(VisualStateManagerEx));

        private readonly struct ResolvedVisualStateSetter
        {
            public ResolvedVisualStateSetter(DependencyObject target, DependencyProperty property, object value)
            {
                Target = target;
                Property = property;
                Value = value;
            }

            public DependencyObject Target { get; }

            public DependencyProperty Property { get; }

            public object Value { get; }

            public SetterKey Key => new SetterKey(Target, Property);
        }

        private readonly struct SetterKey : IEquatable<SetterKey>
        {
            public SetterKey(DependencyObject target, DependencyProperty property)
            {
                Target = target;
                Property = property;
            }

            public DependencyObject Target { get; }

            public DependencyProperty Property { get; }

            public bool Equals(SetterKey other)
            {
                return ReferenceEquals(Target, other.Target) && Property == other.Property;
            }

            public override bool Equals(object obj)
            {
                return obj is SetterKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                return (Target?.GetHashCode() ?? 0) ^ (Property?.GetHashCode() ?? 0);
            }
        }

        private sealed class SetterValueStore
        {
            public void ChangeState(VisualStateGroup group, IReadOnlyList<ResolvedVisualStateSetter> newSetters)
            {
                var affectedKeys = new HashSet<SetterKey>();

                for (int i = _activeSetters.Count - 1; i >= 0; i--)
                {
                    if (ReferenceEquals(_activeSetters[i].Group, group))
                    {
                        affectedKeys.Add(_activeSetters[i].Setter.Key);
                        _activeSetters.RemoveAt(i);
                    }
                }

                foreach (ResolvedVisualStateSetter setter in newSetters)
                {
                    SetterKey key = setter.Key;
                    if (!_baseValues.ContainsKey(key))
                    {
                        _baseValues.Add(key, BaseValue.Capture(key.Target, key.Property));
                    }

                    affectedKeys.Add(key);
                    _activeSetters.Add(new ActiveSetter(group, setter));
                }

                foreach (SetterKey key in affectedKeys)
                {
                    ApplyEffectiveValue(key);
                }
            }

            private void ApplyEffectiveValue(SetterKey key)
            {
                for (int i = _activeSetters.Count - 1; i >= 0; i--)
                {
                    ActiveSetter activeSetter = _activeSetters[i];
                    if (activeSetter.Setter.Key.Equals(key))
                    {
                        key.Target.SetValue(key.Property, activeSetter.Setter.Value);
                        return;
                    }
                }

                if (_baseValues.TryGetValue(key, out BaseValue baseValue))
                {
                    baseValue.Restore(key.Target, key.Property);
                    _baseValues.Remove(key);
                }
            }

            private readonly List<ActiveSetter> _activeSetters = new List<ActiveSetter>();
            private readonly Dictionary<SetterKey, BaseValue> _baseValues = new Dictionary<SetterKey, BaseValue>();
        }

        private readonly struct ActiveSetter
        {
            public ActiveSetter(VisualStateGroup group, ResolvedVisualStateSetter setter)
            {
                Group = group;
                Setter = setter;
            }

            public VisualStateGroup Group { get; }

            public ResolvedVisualStateSetter Setter { get; }
        }

        private sealed class BaseValue
        {
            public static BaseValue Capture(DependencyObject target, DependencyProperty property)
            {
                return new BaseValue
                {
                    LocalValue = target.ReadLocalValue(property),
                    EffectiveValue = target.GetValue(property),
                    Binding = BindingOperations.GetBindingBase(target, property)
                };
            }

            public void Restore(DependencyObject target, DependencyProperty property)
            {
                target.ClearValue(property);

                if (Binding != null)
                {
                    BindingOperations.SetBinding(target, property, Binding);
                    return;
                }

                if (LocalValue != DependencyProperty.UnsetValue)
                {
                    try
                    {
                        target.SetValue(property, LocalValue);
                    }
                    catch (InvalidOperationException)
                    {
                        target.SetCurrentValue(property, EffectiveValue);
                    }
                }
            }

            private object LocalValue { get; set; }

            private object EffectiveValue { get; set; }

            private BindingBase Binding { get; set; }
        }

        private readonly struct ParsedTargetPath
        {
            private ParsedTargetPath(string targetName, string propertyPath)
            {
                TargetName = targetName;
                PropertyPath = propertyPath;
            }

            public string TargetName { get; }

            public string PropertyPath { get; }

            public static ParsedTargetPath Parse(VisualStateSetter setter)
            {
                string target = setter.Target?.Trim();
                string property = setter.Property?.Trim();

                if (!string.IsNullOrEmpty(property))
                {
                    return new ParsedTargetPath(target, property);
                }

                if (string.IsNullOrEmpty(target))
                {
                    throw new InvalidOperationException("Visual state setter requires either Target or Property.");
                }

                int separatorIndex = target.IndexOf('.');
                if (separatorIndex < 0)
                {
                    throw new InvalidOperationException(
                        $"Visual state setter target '{target}' must include a property path or be paired with Property.");
                }

                string targetName = target.Substring(0, separatorIndex);
                string propertyPath = target.Substring(separatorIndex + 1);

                if (propertyPath.StartsWith("(", StringComparison.Ordinal))
                {
                    int closeIndex = propertyPath.IndexOf(')');
                    if (closeIndex != propertyPath.Length - 1)
                    {
                        throw new NotSupportedException($"Nested visual state setter target path '{target}' is not supported.");
                    }
                }
                else if (propertyPath.IndexOf('.') >= 0)
                {
                    throw new NotSupportedException($"Nested visual state setter target path '{target}' is not supported.");
                }

                return new ParsedTargetPath(targetName, propertyPath);
            }
        }
    }
}
