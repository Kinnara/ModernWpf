//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Navigation;

namespace ModernWpf.SampleApp.ControlPages
{
    public sealed partial class ListView2Page : ItemsPageBase
    {
        ObservableCollection<Contact> contacts1 = new ObservableCollection<Contact>();
        ObservableCollection<Contact> contacts2 = new ObservableCollection<Contact>();
        ObservableCollection<Contact> contacts3 = new ObservableCollection<Contact>();
        ObservableCollection<Contact> contacts3Filtered = new ObservableCollection<Contact>();

        CollectionViewSource ContactsCVS;

        public ListView2Page()
        {
            InitializeComponent();
            ContactsCVS = (CollectionViewSource)Resources[nameof(ContactsCVS)];
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            //Items = ControlInfoDataSource.Instance.Groups.Take(3).SelectMany(g => g.Items).ToList();
            BaseExample.ItemsSource = await Contact.GetContactsAsync();
            Control2.ItemsSource = await Contact.GetContactsAsync();
            contacts1 = await Contact.GetContactsAsync();

            contacts2.Add(new Contact("John", "Doe", "ABC Printers"));
            contacts2.Add(new Contact("Jane", "Doe", "XYZ Refridgerators"));
            contacts2.Add(new Contact("Santa", "Claus", "North Pole Toy Factory Inc."));

            Control4.ItemsSource = CustomDataObject.GetDataObjects();
            ContactsCVS.Source = await Contact.GetContactsAsync();

            // Initialize list of contacts to be filtered
            contacts3 = await Contact.GetContactsAsync();
            contacts3Filtered = new ObservableCollection<Contact>(contacts3);

            FilteredListView.ItemsSource = contacts3Filtered;
        }

        //===================================================================================================================
        // Selection Modes Example
        //===================================================================================================================
        private void SelectionModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Control2 != null)
            {
                string selectionMode = e.AddedItems[0].ToString();
                switch (selectionMode)
                {
                    case "None":
                        Control2.IsSelectionEnabled = false;
                        break;
                    case "Single":
                        Control2.SelectionMode = SelectionMode.Single;
                        Control2.IsSelectionEnabled = true;
                        break;
                    case "Multiple":
                        Control2.SelectionMode = SelectionMode.Multiple;
                        Control2.IsSelectionEnabled = true;
                        break;
                    case "Extended":
                        Control2.SelectionMode = SelectionMode.Extended;
                        Control2.IsSelectionEnabled = true;
                        break;
                }
            }
        }

        //===================================================================================================================
        // Filtered List Example
        //===================================================================================================================
        private void Remove_NonMatching(IEnumerable<Contact> filteredData)
        {
            for (int i = contacts3Filtered.Count - 1; i >= 0; i--)
            {
                var item = contacts3Filtered[i];
                // If contact is not in the filtered argument list, remove it from the ListView's source.
                if (!filteredData.Contains(item))
                {
                    contacts3Filtered.Remove(item);
                }
            }
        }

        private void AddBack_Contacts(IEnumerable<Contact> filteredData)
        // When a user hits backspace, more contacts may need to be added back into the list
        {
            foreach (var item in filteredData)
            {
                // If item in filtered list is not currently in ListView's source collection, add it back in
                if (!contacts3Filtered.Contains(item))
                {
                    contacts3Filtered.Add(item);
                }
            }
        }

        private void OnFilterChanged(object sender, TextChangedEventArgs args)
        {
            // Linq query that selects only items that return True after being passed through Filter function
            var filtered = contacts3.Where(contact => Filter(contact));
            Remove_NonMatching(filtered);
            AddBack_Contacts(filtered);
        }

        private bool Filter(Contact contact)
        {
            // When the text in any filter is changed, contact list is ran through all three filters to make sure
            // they can properly interact with each other (i.e. they can all be applied at the same time).

            return contact.FirstName.IndexOf(FilterByFirstName.Text, StringComparison.InvariantCultureIgnoreCase) > -1 &&
                   contact.LastName.IndexOf(FilterByLastName.Text, StringComparison.InvariantCultureIgnoreCase) > -1 &&
                   contact.Company.IndexOf(FilterByCompany.Text, StringComparison.InvariantCultureIgnoreCase) > -1;
        }
    }

    public class NoPaddingFlowDocument : FlowDocument
    {
        static NoPaddingFlowDocument()
        {
            PagePaddingProperty.OverrideMetadata(typeof(NoPaddingFlowDocument), new FrameworkPropertyMetadata { CoerceValueCallback = CoercePagePadding });
        }

        public NoPaddingFlowDocument()
        {
            SetResourceReference(StyleProperty, typeof(FlowDocument));
        }

        private static object CoercePagePadding(DependencyObject d, object baseValue)
        {
            return new Thickness();
        }
    }
}
