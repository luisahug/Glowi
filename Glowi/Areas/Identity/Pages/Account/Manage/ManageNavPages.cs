using Microsoft.AspNetCore.Mvc.Rendering;

namespace Glowi.Areas.Identity.Pages.Account.Manage;

// Marca a aba ativa do menu de "Minha conta"
public static class ManageNavPages
{
    public static string Index => "Index";
    public static string Email => "Email";
    public static string ChangePassword => "ChangePassword";
    public static string PersonalData => "PersonalData";
    public static string DeletePersonalData => "DeletePersonalData";

    public static string? IndexNavClass(ViewContext viewContext) => PageNavClass(viewContext, Index);
    public static string? EmailNavClass(ViewContext viewContext) => PageNavClass(viewContext, Email);
    public static string? ChangePasswordNavClass(ViewContext viewContext) => PageNavClass(viewContext, ChangePassword);
    public static string? PersonalDataNavClass(ViewContext viewContext) => PageNavClass(viewContext, PersonalData);

    private static string? PageNavClass(ViewContext viewContext, string page)
    {
        var activePage = viewContext.ViewData["ActivePage"] as string
            ?? Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
        // "Excluir conta" fica dentro de "Dados pessoais"
        if (activePage == DeletePersonalData) activePage = PersonalData;
        return string.Equals(activePage, page, StringComparison.OrdinalIgnoreCase) ? "active" : null;
    }
}
