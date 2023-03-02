using System;
using AvaloniaStyles.Controls.FastTableView;
using WDE.DatabaseEditors.Models;
using WDE.DatabaseEditors.ViewModels.MultiRow;

namespace WDE.DatabaseEditors.Avalonia.Views.MultiRow;

public class RowFilterPredicate : IRowFilterPredicate
{
    public bool IsVisible(ITableRow row, object? searchTextObj)
    {
        if (searchTextObj is not string searchText ||
            string.IsNullOrWhiteSpace(searchText))
            return true;

        if (row is not DatabaseEntityViewModel entity)
            return true;

        long? searchTextNum = null;
        if (long.TryParse(searchText, out var searchTextLong))
            searchTextNum = searchTextLong;

        foreach (var cell in entity.Cells)
        {
            if (searchTextNum.HasValue)
            {
                if (cell.ParameterValue is not IParameterValue<long> value)
                    continue;

                if (value.Value == searchTextNum.Value)
                    return true;
            }
            else
            {
                if (cell.ToString() is not { } value)
                    continue;

                if (value.Contains(searchText, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }
        }

        return false;
    }
}