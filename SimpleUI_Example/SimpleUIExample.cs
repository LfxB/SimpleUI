using GTA;
using GTA.Native;
using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Drawing;
using SimpleUI;

namespace SimpleUIExample
{
    public class SimpleUIExample : Script
    {
        enum TestEnum
        {
            enum1 = 0,
            enum2,
            enum3
        }

        MenuPool _menuPool;
        UIMenu mainMenu;
        UIMenu subMenu;

        UIMenuItem itemSelectFunction;
        UIMenuItem itemBoolControl;
        UIMenuSubsectionItem subsectionItem;
        UIMenuNumberValueItem itemIntegerControl;
        UIMenuNumberValueItem itemFloatControl;
        UIMenuNumberValueItem itemEnumControl;
        UIMenuSubsectionItem subsectionItem2;
        UIMenuListItem itemListControl;
        UIMenuListItem itemListControlAdvanced;
        UIMenuItem itemAddPerson;
        UIMenuItem itemRemoveLastPerson;

        UIMenuItem submenuItem1;
        UIMenuItem submenuItem2;

        bool testBool;
        int testInt;
        float testFloat;
        TestEnum testEnum = TestEnum.enum1;
        List<dynamic> testListString = new List<dynamic>()
        {
            "List Item 1",
            "List Item 2",
            "List Item 3",
            "List Item 4"
        };
        List<dynamic> testListAdvanced = new List<dynamic>()
        {
            new Person("Michael", "Scott", 8008),
            new Person("Dwight", "Schrute", 1337),
            new Person("Stanley", "Hudson", 101)
        };

        public SimpleUIExample()
        {
            //Setup menu
            InitMenu();

            Tick += OnTick;
            KeyUp += OnKeyUp;

            Interval = 0;
        }

        void InitMenu()
        {
            // First initialize an instance of a MenuPool.
            // A MenuPool object will manage all the interconnected
            // menus that you add to it.
            _menuPool = new MenuPool();

            // Initialize a menu, with name "Main Menu"
            mainMenu = new UIMenu("Main Menu");
            // Add mainMenu to _menuPool
            _menuPool.AddMenu(mainMenu);

            // Let's set the colors of the menu before adding other menus
            // so that submenus will also have the same color scheme.
            // Requires a reference to System.Drawing
            mainMenu.TitleColor = Color.FromArgb(255, 237, 90, 90);
            mainMenu.TitleBackgroundColor = Color.FromArgb(240, 0, 0, 0);
            mainMenu.TitleUnderlineColor = Color.FromArgb(255, 237, 90, 90);
            mainMenu.DefaultBoxColor = Color.FromArgb(160, 0, 0, 0);
            mainMenu.DefaultTextColor = Color.FromArgb(230, 255, 255, 255);
            mainMenu.HighlightedBoxColor = Color.FromArgb(130, 237, 90, 90);
            mainMenu.HighlightedItemTextColor = Color.FromArgb(255, 255, 255, 255);
            mainMenu.DescriptionBoxColor = Color.FromArgb(255, 0, 0, 0);
            mainMenu.DescriptionTextColor = Color.FromArgb(255, 255, 255, 255);
            mainMenu.SubsectionDefaultBoxColor = Color.FromArgb(160, 0, 0, 0);
            mainMenu.SubsectionDefaultTextColor = Color.FromArgb(180, 255, 255, 255);

            // A string attached to the end of submenu's menu item text
            // to indicate that the item leads to a submenu.
            _menuPool.SubmenuItemIndication = "  ~r~>";

            #region SUBMENU_SETUP

            // Initialize another menu, with name "Submenu"
            subMenu = new UIMenu("Submenu");
            // Add subMenu to _menuPool as a child menu of mainMenu.
            // This will create a menu item in mainMenu with the name "Go to Submenu",
            // and selecting it will bring you to the submenu.
            _menuPool.AddSubMenu(subMenu, mainMenu, "Go to Submenu");

            // Initialize an item called "Submenu Item 1"
            // and add it to the submenu.
            submenuItem1 = new UIMenuItem("Submenu Item 1");
            subMenu.AddMenuItem(submenuItem1);

            // Same as above
            submenuItem2 = new UIMenuItem("Submenu Item 2");
            subMenu.AddMenuItem(submenuItem2);

            #endregion

            // A UIMenuSubsectionItem is essentially just a splitter.
            subsectionItem = new UIMenuSubsectionItem("--- Splitter ---");
            // Add subsectionItem to the mainMenu.
            // It will appear after the subMenu item,
            // since this is the order we are affecting mainMenu.
            mainMenu.AddMenuItem(subsectionItem);

            // Just adding some more items to mainMenu.
            // Second param is the default value.
            // Third param is a description that appears at the bottom
            // of the menu.
            // UIMenuNumberValueItem is just like UIMenuItem but with "<" and ">"
            // wrapped around the value.

            itemSelectFunction = new UIMenuItem("Select me!", null, "Select me to show a subtitle.");
            mainMenu.AddMenuItem(itemSelectFunction);

            itemBoolControl = new UIMenuItem("Bool Item", testBool, "This item controls a bool.");
            mainMenu.AddMenuItem(itemBoolControl);

            itemIntegerControl = new UIMenuNumberValueItem("Integer Item", testInt, "This item controls an integer.");
            mainMenu.AddMenuItem(itemIntegerControl);

            itemFloatControl = new UIMenuNumberValueItem("Float Item", testFloat, "This item controls a float.");
            mainMenu.AddMenuItem(itemFloatControl);

            itemEnumControl = new UIMenuNumberValueItem("Enum Item", testEnum, "This item controls an enum.");
            mainMenu.AddMenuItem(itemEnumControl);

            subsectionItem2 = new UIMenuSubsectionItem("--- List Stuff ---");
            mainMenu.AddMenuItem(subsectionItem2);

            // the 3rd param must be of type List<dynamic>
            // or you will get a compile time error.
            itemListControl = new UIMenuListItem("List Item", "This item can output an object from a list.", testListString);
            mainMenu.AddMenuItem(itemListControl);

            itemListControlAdvanced = new UIMenuListItem("People", "A list of people", testListAdvanced);
            mainMenu.AddMenuItem(itemListControlAdvanced);

            itemAddPerson = new UIMenuItem("Add a person");
            mainMenu.AddMenuItem(itemAddPerson);

            itemRemoveLastPerson = new UIMenuItem("Remove last person");
            mainMenu.AddMenuItem(itemRemoveLastPerson);

            // Now let's create some events.
            // All events are in the UIMenu class.
            // You can create a specific or anonymous method.

            // Let's subscribe mainMenu's OnItemSelect event to an anonymous method.
            // This method will be executed whenever you press Enter, Numpad5, or
            // the Select button on a gamepad while mainMenu is open.
            mainMenu.OnItemSelect += (sender, selectedItem, index) =>
            {
                // Check which item is selected.
                if (selectedItem == itemSelectFunction)
                {
                    UI.ShowSubtitle("Hi! I'm testing SimpleUI's OnItemSelect event!");
                }
                else if (selectedItem == itemBoolControl)
                {
                    // ControlBoolValue is an easy way to let a menu item control
                    // a specific bool with one line of code.
                    // In this example, we will control the var "testBool" with
                    // the "itemBoolControl" menu item.
                    mainMenu.ControlBoolValue(ref testBool, itemBoolControl);
                }
                else if (selectedItem == itemAddPerson)
                {
                    string fname = Game.GetUserInput("FirstName", 999);
                    if (String.IsNullOrWhiteSpace(fname)) return;

                    string lname = Game.GetUserInput("LastName", 999);
                    if (String.IsNullOrWhiteSpace(lname)) return;

                    string input = Game.GetUserInput("ID", 999);
                    if (String.IsNullOrWhiteSpace(lname)) return;

                    int id;
                    bool idParsed = int.TryParse(input, out id);

                    if (!idParsed) return;

                    testListAdvanced.Add(new Person(fname, lname, id));

                    // Call this after modifying your list or you may
                    // get an out of bounds error.
                    itemListControlAdvanced.SaveListUpdateFromOutOfBounds();

                    UI.ShowSubtitle(fname + " " + lname + " added to list!");
                }
                else if (selectedItem == itemRemoveLastPerson)
                {
                    if (testListAdvanced.Count > 1)
                    {
                        UI.ShowSubtitle(testListAdvanced[testListAdvanced.Count - 1].ToString() + " removed from list!");

                        // Don't want to use LINQ for just this one line..
                        testListAdvanced.RemoveAt(testListAdvanced.Count - 1);

                        itemListControlAdvanced.SaveListUpdateFromOutOfBounds();
                    }
                    else
                    {
                        UI.ShowSubtitle("There is only one person left!");
                    }
                }
            };

            // Let's subscribe subMenu's WhileItemHighlight event to an anonymous method
            // This method will be executed continuously while subMenu is open.
            subMenu.WhileItemHighlight += (sender, selectedItem, index) =>
            {
                // Check which item is selected.
                if (selectedItem == submenuItem1)
                {
                    UI.ShowSubtitle("Highlighting subMenu's Item 1");
                }
                else if (selectedItem == submenuItem2)
                {
                    UI.ShowSubtitle("Highlighting subMenu's Item 2");
                }
            };

            // Let's subscribe mainMenu's OnItemLeftRight event to the method
            // "MainMenu_OnItemLeftRight"
            // This method will then be executed whenever you press left or right
            // while mainMenu is open.
            mainMenu.OnItemLeftRight += MainMenu_OnItemLeftRight;

            // That's it for this example setup!
            // SimpleUI also supports scrolling, so you can add as many items
            // or submenus as you'd like.
            // SimpleUI also supports dynamic hiding/showing of menu items,
            // and Dispose methods for items and menus, allowing easy modification
            // after the initial setup. Explore using Intellisense!
        }

        private void MainMenu_OnItemLeftRight(UIMenu sender, UIMenuItem selectedItem, int index, UIMenu.Direction direction)
        {
            // Check which item is selected.
            if (selectedItem == itemIntegerControl)
            {
                // ControlIntValue is an easy way to let a menu item control
                // a specific int with one line of code.
                // In this example, we will control the var "testInt" with
                // the "itemIntegerControl" menu item.
                // The params that follow are explained with intellisense.
                mainMenu.ControlIntValue(ref testInt, itemIntegerControl, direction, 1, 5, true, 0, 100);

                UI.ShowSubtitle("You pressed " + (direction == UIMenu.Direction.Left ? "Left" : "Right") + " while highlighting Integer Item!");
            }
            else if (selectedItem == itemFloatControl)
            {
                // ControlFloatValue is an easy way to let a menu item control
                // a specific float with one line of code.
                // In this example, we will control the var "testFloat" with
                // the "itemFloatControl" menu item.
                // The params that follow are explained with intellisense.
                mainMenu.ControlFloatValue(ref testFloat, itemFloatControl, direction, 0.5f, 1f, 2, true, 0f, 10f);

                UI.ShowSubtitle("You pressed " + (direction == UIMenu.Direction.Left ? "Left" : "Right") + " while highlighting Float Item!");
            }
            else if (selectedItem == itemEnumControl)
            {
                // ControlEnumValue is an easy way to let a menu item control
                // a specific enum with one line of code.
                // In this example, we will control the var "testEnum" with
                // the "itemEnumControl" menu item.
                mainMenu.ControlEnumValue(ref testEnum, itemEnumControl, direction);

                UI.ShowSubtitle("You pressed " + (direction == UIMenu.Direction.Left ? "Left" : "Right") + " while highlighting Enum Item!");
            }
            else if (selectedItem == itemListControl)
            {
                // An item of type UIMenuListItem is automatically controlled by the menu.
                UI.ShowSubtitle("\"" + itemListControl.CurrentListItem.ToString() + "\" is selected.");
            }
            else if (selectedItem == itemListControlAdvanced)
            {
                // UIMenuListItem.CurrentListItem will return the actual selected object
                // in the list. You must cast it to the actual object type. Ex:
                // Person p = (Person)list.CurrentListItem;
                UI.ShowSubtitle("\"" + itemListControlAdvanced.CurrentListItem.ToString() + "\" is selected.");
            }
        }

        void OnTick(object sender, EventArgs e)
        {
            // Process all the menus in _menuPool
            _menuPool.ProcessMenus();
        }

        void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.K)
            {
                // Open / close the menu with the K key.
                _menuPool.OpenCloseLastMenu();
            }
        }
    }

    public class Person
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public Person(string fname, string lname, int id)
        {
            FirstName = fname;
            LastName = lname;
            Id = id;
        }

        // Override the ToString() method so that
        // the menu will display exactly what you
        // want it to display.
        public override string ToString()
        {
            return FirstName + " " + LastName + ", ID:" + Id;
        }
    }
}