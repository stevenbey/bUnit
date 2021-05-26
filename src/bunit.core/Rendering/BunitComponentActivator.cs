#if NET5_0_OR_GREATER
using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components;

namespace Bunit.Rendering
{
	/// <summary>
	/// Represents the bUnit <see cref="IComponentActivator"/>.
	/// </summary>
	public sealed class BunitComponentActivator : IComponentActivator
	{
		private readonly ComponentFactoryCollection factories;
		private readonly IComponentActivator? externalComponentActivator;

		/// <summary>
		/// Initializes a new instance of the <see cref="BunitComponentActivator"/> class.
		/// </summary>
		/// <param name="factories">Test factories to use to create components with.</param>
		/// <param name="externalComponentActivator">Optional external component activator to use as a fall-back.</param>
		public BunitComponentActivator(ComponentFactoryCollection factories, IComponentActivator? externalComponentActivator)
		{
			this.factories = factories ?? throw new ArgumentNullException(nameof(factories));
			this.externalComponentActivator = externalComponentActivator;
		}

		/// <inheritdoc/>
		public IComponent CreateInstance([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type componentType)
		{
			if (componentType is null)
			{
				throw new ArgumentNullException(nameof(componentType));
			}

			if (!typeof(IComponent).IsAssignableFrom(componentType))
			{
				throw new ArgumentException($"The type {componentType.FullName} does not implement {nameof(IComponent)}.", nameof(componentType));
			}

			// The FragmentContainer is a bUnit component added to the
			// render tree to separate the components from the TestContextBase.RenderTree
			// and the components in the render fragment being rendered.
			// It should never be replaced by another component, as
			// this would break bUnits ability to detect the start
			// of the component under test.
			if (typeof(FragmentContainer) == componentType)
				return new FragmentContainer();

			for (int i = factories.Count - 1; i >= 0; i--)
			{
				var factory = factories[i];
				if (factory.CanCreate(componentType))
				{
					return factory.Create(componentType);
				}
			}

			return externalComponentActivator is not null
				? externalComponentActivator.CreateInstance(componentType)
			    : (IComponent)Activator.CreateInstance(componentType)!;
		}
	}
}
#endif
