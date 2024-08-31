using Microsoft.CodeAnalysis;

namespace Microsoft.Maui.Controls.BindingSourceGen;

internal static class ITypeSymbolExtensions
{
	internal static bool IsTypeNullable(this ITypeSymbol typeInfo, bool enabledNullable)
	{
		if (!enabledNullable && typeInfo.IsReferenceType)
		{
			return true;
		}

		return typeInfo.IsNullableValueType() || typeInfo.IsNullableReferenceType();
	}

	internal static TypeDescription CreateTypeDescription(this ITypeSymbol typeSymbol, bool enabledNullable)
	{
		var isNullable = IsTypeNullable(typeSymbol, enabledNullable);
		return new TypeDescription(
			GlobalName: GetGlobalName(typeSymbol, isNullable, typeSymbol.IsValueType),
			IsNullable: isNullable,
			IsGenericParameter: typeSymbol.Kind == SymbolKind.TypeParameter, //TODO: Add support for generic parameters
			IsValueType: typeSymbol.IsValueType);
	}

	private static bool IsNullableValueType(this ITypeSymbol typeInfo) =>
		typeInfo is INamedTypeSymbol namedTypeSymbol
			&& namedTypeSymbol.IsGenericType
			&& namedTypeSymbol.ConstructedFrom.SpecialType == SpecialType.System_Nullable_T;

	private static bool IsNullableReferenceType(this ITypeSymbol typeInfo) =>
		typeInfo.IsReferenceType && typeInfo.NullableAnnotation == NullableAnnotation.Annotated;


	private static string GetGlobalName(this ITypeSymbol typeSymbol, bool isNullable, bool isValueType)
	{
		if (isNullable && isValueType)
		{
			// Strips the "?" from the type name
			return ((INamedTypeSymbol)typeSymbol).TypeArguments[0].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
		}

		return typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
	}
}

internal static class ISymbolExtensions
{
	internal static bool IsAccessible(this ISymbol symbol) =>
		symbol.DeclaredAccessibility == Accessibility.Public
		|| symbol.DeclaredAccessibility == Accessibility.Internal
		|| symbol.DeclaredAccessibility == Accessibility.ProtectedOrInternal;

	internal static AccessorKind ToAccessorKind(this ISymbol symbol)
	{
		return symbol switch
		{
			IFieldSymbol _ => AccessorKind.Field,
			IPropertySymbol _ => AccessorKind.Property,
			_ => throw new ArgumentException("Symbol is not a field or property.", nameof(symbol))
		};
	}
}

public enum AccessorKind
{
	Field,
	Property,
}