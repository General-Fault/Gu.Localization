﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StaticExtension.cs" company="">
//   
// </copyright>
// <summary>
//   Implements a markup extension that returns static field and property references.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Gu.Wpf.Localization
{
    using System;
    using System.ComponentModel;
    using System.Linq;
    using System.Resources;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Markup;
    using System.Xaml;

    using Gu.Localization;
    using Gu.Localization.Properties;
    using Gu.Wpf.Localization.Internals;

    using ResourceKey = Gu.Wpf.Localization.Internals.ResourceKey;

    /// <summary>
    /// Implements a markup extension that translates resources.
    /// The reason for the name StaticExtension is that it tricks Resharper into providing Intellisense.
    /// l:Static p:Resources.YourKey
    /// </summary>
    [MarkupExtensionReturnType(typeof(string))]
    [ContentProperty("Member"), DefaultProperty("Member")]
    [TypeConverter(typeof(StaticExtensionConverter))]
    public class StaticExtension : MarkupExtension
    {
        /// <summary>
        /// The _xaml type resolver.
        /// </summary>
        private IXamlTypeResolver _xamlTypeResolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Gu.Wpf.Localization.StaticExtension"/> class using the provided <paramref name="member"/> string.
        /// </summary>
        /// <param name="member">
        /// A string that identifies the member to make a reference to. This string uses the format prefix:typeName.fieldOrPropertyName. prefix is the mapping prefix for a XAML namespace, and is only required to reference static values that are not mapped to the default XAML namespace.
        /// </param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="member"/> is null.
        /// </exception>
        public StaticExtension(string member)
        {
            Member = member;
        }

        [ConstructorArgument("member")]
        public string Member { get; set; }

        public ResourceManager ResourceManager { get; set; }

        /// <summary>
        /// Returns an object value to set on the property where you apply this extension. For <see cref="T:System.Windows.Markup.StaticExtension"/>, the return value is the static value that is evaluated for the requested static member.
        /// </summary>
        /// <returns>
        /// The static value to set on the property where the extension is applied.
        /// </returns>
        /// <param name="serviceProvider">
        /// An object that can provide services for the markup extension. The service provider is expected to provide a service that implements a type resolver (<see cref="T:System.Windows.Markup.IXamlTypeResolver"/>).
        /// </param>
        /// <exception cref="T:System.InvalidOperationException">
        /// The <paramref name="member"/> value for the extension is null at the time of evaluation.
        /// </exception>
        /// <exception cref="T:System.ArgumentException">
        /// Some part of the <paramref name="member"/> string did not parse properly-or-<paramref name="serviceProvider"/> did not provide a service for <see cref="T:System.Windows.Markup.IXamlTypeResolver"/>-or-<paramref name="member"/> value did not resolve to a static member.
        /// </exception>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="serviceProvider"/> is null.
        /// </exception>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException("serviceProvider");
            }

            if (string.IsNullOrEmpty(Member))
            {
                throw new InvalidOperationException("MarkupExtensionStaticMember");
            }
            if (DesignMode.IsDesignMode)
            {
                return DesigntimeSandbox.ProvideValue(serviceProvider, Member);
            }
            try
            {
                if (DesignMode.IsDesignMode && serviceProvider.IsInTemplate())
                {
                    _xamlTypeResolver = serviceProvider.XamlTypeResolver();
                    return this;
                }

                if (_xamlTypeResolver == null)
                {
                    _xamlTypeResolver = serviceProvider.XamlTypeResolver();
                }
                var resourceKey = ResourceKey.Resolve(Member, _xamlTypeResolver, DesignMode.IsDesignMode);
                if (resourceKey.HasError)
                {
                    return string.Format(Resources.UnknownErrorFormat, Member);
                }
                var translation = new Translation(resourceKey.ResourceManager, resourceKey.Key);
                var binding = new Binding(ExpressionHelper.PropertyName(() => translation.Translated))
                {
                    Mode = BindingMode.OneWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                    Source = translation
                };
                var provideValue = binding.ProvideValue(serviceProvider);
                return provideValue;
            }
            catch (Exception exception)
            {
                //if (DesignMode.IsDesignMode)
                //{
                //    if (exception is XamlParseException)
                //    {
                //        return Member;
                //    }
                //    else
                //    {
                //        throw;
                //    }
                //}

                return string.Format(Resources.UnknownErrorFormat, Member);
            }
        }
    }
}
