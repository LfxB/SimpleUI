using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Drawing;
using GTA;
using GTA.Native;
using Control = GTA.Control;

namespace SimpleUI
{
    class MenuPool
    {
        List<UIMenu> _menuList = new List<UIMenu>();
        public UIMenu LastUsedMenu { get; set; }

        public List<UIMenu> UIMenuList
        {
            get { return _menuList; }
            set { _menuList = value; }
        }

        public void AddMenu(UIMenu menu)
        {
            _menuList.Add(menu);
            if (_menuList.Count == 1) { LastUsedMenu = menu; }
        }

        /// <summary>
        /// Adds a submenu to a parent menu and to the MenuPool. Returns UIMenuItem that links the parent menu to the submenu.
        /// </summary>
        /// <param name="SubMenu">The submenu</param>
        /// <param name="ParentMenu">The parent menu.</param>
        /// <param name="text">The text of the menu item in the parent menu that leads to the submenu when entered.</param>
        public void AddSubMenu(UIMenu SubMenu, UIMenu ParentMenu, string text, bool UseSameColorsAsParent = true)
        {
            AddMenu(SubMenu);
            /*SubMenu.ParentMenu = ParentMenu;
            ParentMenu.NextMenu = SubMenu;*/
            var item = new UIMenuItem(text + "  ~r~>"); //colour codes: gtaforums.com/topic/820813-displaying-help-text/?p=1067993556
            //ParentMenu.BindingMenuItem = BindingItem;
            ParentMenu.AddMenuItem(item);
            //ParentMenu.BindingMenuItem = item;
            ParentMenu.BindItemToSubmenu(SubMenu, item);

            if (UseSameColorsAsParent)
            {
                SubMenu.TitleColor = ParentMenu.TitleColor;
                SubMenu.TitleUnderlineColor = ParentMenu.TitleUnderlineColor;
                SubMenu.TitleBackgroundColor = ParentMenu.TitleBackgroundColor;

                SubMenu.DefaultTextColor = ParentMenu.DefaultTextColor;
                SubMenu.DefaultBoxColor = ParentMenu.DefaultBoxColor;
                SubMenu.HighlightedItemTextColor = ParentMenu.HighlightedItemTextColor;
                SubMenu.HighlightedBoxColor = ParentMenu.HighlightedBoxColor;

                SubMenu.DescriptionTextColor = ParentMenu.DescriptionTextColor;
                SubMenu.DescriptionBoxColor = ParentMenu.DescriptionBoxColor;
            }
        }

        /// <summary>
        /// Adds a submenu to a parent menu and to the MenuPool. Returns UIMenuItem that links the parent menu to the submenu.
        /// </summary>
        /// <param name="SubMenu">The submenu</param>
        /// <param name="ParentMenu">The parent menu.</param>
        /// <param name="text">The text of the menu item in the parent menu that leads to the submenu when entered.</param>
        /// <param name="description">The description of the menu item that leads to the submenu when entered.</param>
        public void AddSubMenu(UIMenu SubMenu, UIMenu ParentMenu, string text, string description, bool UseSameColorsAsParent = true)
        {
            AddMenu(SubMenu);
            //SubMenu.ParentMenu = ParentMenu;
            //ParentMenu.NextMenu = SubMenu;
            var item = new UIMenuItem(text + "  ~r~>", null, description);
            //ParentMenu.BindingMenuItem = BindingItem;
            ParentMenu.AddMenuItem(item);
            //ParentMenu.BindingMenuItem = item;
            ParentMenu.BindItemToSubmenu(SubMenu, item);

            if (UseSameColorsAsParent)
            {
                SubMenu.TitleColor = ParentMenu.TitleColor;
                SubMenu.TitleUnderlineColor = ParentMenu.TitleUnderlineColor;
                SubMenu.TitleBackgroundColor = ParentMenu.TitleBackgroundColor;

                SubMenu.DefaultTextColor = ParentMenu.DefaultTextColor;
                SubMenu.DefaultBoxColor = ParentMenu.DefaultBoxColor;
                SubMenu.HighlightedItemTextColor = ParentMenu.HighlightedItemTextColor;
                SubMenu.HighlightedBoxColor = ParentMenu.HighlightedBoxColor;

                SubMenu.DescriptionTextColor = ParentMenu.DescriptionTextColor;
                SubMenu.DescriptionBoxColor = ParentMenu.DescriptionBoxColor;
            }
        }

        /// <summary>
        /// Draws all visible menus.
        /// </summary>
        public void Draw()
        {
            foreach (var menu in _menuList.Where(menu => menu.IsVisible))
            {
                menu.Draw();
                SetLastUsedMenu(menu);
            }
        }

        /// <summary>
        /// Set the last used menu.
        /// </summary>
        public void SetLastUsedMenu(UIMenu menu)
        {
            LastUsedMenu = menu;
        }

        /// <summary>
        /// Process all of your menus' functions. Call this in a tick event.
        /// </summary>
        public void ProcessMenus()
        {
            if (LastUsedMenu == null)
            {
                LastUsedMenu = _menuList[0];
            }
            Draw();
        }

        /// <summary>
        /// Checks if any menu is currently visible.
        /// </summary>
        /// <returns>true if at least one menu is visible, false if not.</returns>
        public bool IsAnyMenuOpen()
        {
            return _menuList.Any(menu => menu.IsVisible);
        }

        /// <summary>
        /// Closes all of your menus.
        /// </summary>
        public void CloseAllMenus()
        {
            foreach (var menu in _menuList.Where(menu => menu.IsVisible))
            {
                menu.IsVisible = false;
            }
        }

        public void RemoveAllMenus()
        {
            _menuList.Clear();
        }

        public void OpenCloseLastMenu()
        {
            if (this.IsAnyMenuOpen())
            {
                this.CloseAllMenus();
            }
            else
            {
                this.LastUsedMenu.IsVisible = !this.LastUsedMenu.IsVisible;
            }
        }
    }

    public delegate void ItemSelectEvent(UIMenu sender, UIMenuItem selectedItem, int index);
    public delegate void ItemLeftRightEvent(UIMenu sender, UIMenuItem selectedItem, int index, bool left);

    public class UIMenu
    {
        public UIMenu ParentMenu { get; set; }
        public UIMenuItem ParentItem { get; set; }
        public UIMenu NextMenu { get; set; }
        public UIMenuItem BindingMenuItem { get; set; }

        public int SelectedIndex = 0;
        public bool IsVisible = false;
        public string Title { get; set; }
        public UIMenuItem SelectedItem;
        protected List<UIMenuItem> _itemList = new List<UIMenuItem>();
        List<BindedItem> _bindedList = new List<BindedItem>();
        public Dictionary<UIMenuItem, UIMenu> Binded { get; }

        DateTime InputTimer;
        static int InputWait = 80;

        public bool UseEventBasedControls = true;

        /// <summary>
        /// Called when user selects a simple item.
        /// </summary>
        public event ItemSelectEvent OnItemSelect;

        /// <summary>
        /// Called when user presses left or right over a simple item.
        /// </summary>
        public event ItemLeftRightEvent OnItemLeftRight;

        public int menuXPos = 38; //pixels from the top
        public int menuYPos = 38; //pixels from the left
        public int boxWidth = 500; //width in pixels
        public int boxScrollWidth = 4; //width in pixels
        public int boxTitleHeight = 76; //height in pixels
        public int boxUnderlineHeight = 1; //height in pixels
        public int boxHeight = 38; //height in pixels

        /*Title Formatting*/
        public Color TitleColor = Color.FromArgb(255, 255, 255, 255);
        public Color TitleUnderlineColor = Color.FromArgb(140, 0, 255, 255);
        public Color TitleBackgroundColor = Color.FromArgb(144, 0, 0, 0);

        /*Title*/
        public float TitleFontSize;
        protected float yPosTitleBG;
        protected float yPosTitleText;
        protected float TitleBGHeight;
        protected float UnderlineHeight;
        protected float yPosUnderline;

        /*UIMenuItem Formatting*/
        public Color DefaultTextColor = Color.FromArgb(255, 255, 255, 255);
        public Color DefaultBoxColor = Color.FromArgb(144, 0, 0, 0);
        public Color HighlightedItemTextColor = Color.FromArgb(255, 0, 255, 255);
        public Color HighlightedBoxColor = Color.FromArgb(255, 0, 0, 0);

        /*Rectangle box for UIMenuItem objects*/
        protected float xPosBG;
        protected float yPosItemBG;
        protected float MenuBGWidth;
        protected float heightItemBG;
        protected float posMultiplier;

        protected float ItemTextFontSize;
        protected GTA.Font ItemTextFontType;
        protected float xPosItemText;
        protected float xPosRightEndOfMenu;
        protected float xPosItemValue;
        protected float yPosItem;
        protected float yTextOffset;

        protected float ScrollBarWidth;
        protected float xPosScrollBar;

        /*Description Formatting*/
        public Color DescriptionTextColor = Color.FromArgb(255, 0, 0, 0);
        public Color DescriptionBoxColor = Color.FromArgb(150, 0, 255, 255);

        /*Scroll or nah?*/
        bool UseScroll = true;
        int YPosBasedOnScroll;
        int YPosDescBasedOnScroll;
        float YPosSmoothScrollBar;
        protected int MaxItemsOnScreen = 15;
        protected int minItem = 0;
        protected int maxItem = 14; //must always be 1 less than MaxItemsOnScreen

        private string AUDIO_LIBRARY = "HUD_FRONTEND_DEFAULT_SOUNDSET";

        private string AUDIO_UPDOWN = "NAV_UP_DOWN";
        private string AUDIO_LEFTRIGHT = "NAV_LEFT_RIGHT";
        private string AUDIO_SELECT = "SELECT";
        private string AUDIO_BACK = "BACK";

        //protected event KeyEventHandler KeyUp;
        //bool AcceptPressed;
        //bool CancelPressed;

        public UIMenu(string title)
        {
            Title = title;

            TitleFontSize = 0.9f; //TitleFont = 1.1f; for no-value fit.
            ItemTextFontSize = 0.452f;
            ItemTextFontType = GTA.Font.ChaletComprimeCologne;

            CalculateMenuPositioning();

            //KeyUp += UIMenu_KeyUp;
        }

        /*private void UIMenu_KeyUp(object sender, KeyEventArgs e)
        {
            if (IsVisible)
            {
                if (e.KeyCode == Keys.NumPad5 || e.KeyCode == Keys.Enter)
                {
                    AcceptPressed = true;
                    UI.ShowSubtitle("HI");
                }

                if (e.KeyCode == Keys.NumPad0 || e.KeyCode == Keys.Back)
                {
                    CancelPressed = true;
                }
            }
        }*/

        public virtual void CalculateMenuPositioning()
        {
            const float height = 1080f;
            float ratio = (float)Game.ScreenResolution.Width / Game.ScreenResolution.Height;
            var width = height * ratio;

            TitleBGHeight = boxTitleHeight / height; //0.046f
            yPosTitleBG = ((menuYPos) / height) + TitleBGHeight * 0.5f;
            MenuBGWidth = boxWidth / width; //MenuBGWidth = 0.24f; for no-value fit.
            xPosBG = (menuXPos / width) + MenuBGWidth * 0.5f; //xPosBG = 0.13f; for no-value fit.
            xPosItemText = ((menuXPos + 10) / width);
            heightItemBG = boxHeight / height;
            UnderlineHeight = boxUnderlineHeight / height; //0.002f;
            posMultiplier = boxHeight / height;
            yTextOffset = 0.015f; //offset between text pos and box pos. yPosItemBG - yTextOffset
            ScrollBarWidth = boxScrollWidth / width;

            yPosTitleText = yPosTitleBG - (TitleFontSize / 35f);
            yPosUnderline = yPosTitleBG + (TitleBGHeight / 2) + (UnderlineHeight / 2);
            yPosItemBG = yPosUnderline + (UnderlineHeight / 2) + (heightItemBG / 2); //0.0655f;
            yPosItem = yPosItemBG - (ItemTextFontSize / 30.13f);
            //xPosItemText = xPosBG - (MenuBGWidth / 2) + 0.0055f;
            xPosRightEndOfMenu = xPosBG + MenuBGWidth / 2; //will Right Justify
            xPosScrollBar = xPosRightEndOfMenu - (ScrollBarWidth / 2);
            xPosItemValue = xPosScrollBar - (ScrollBarWidth / 2);
            YPosSmoothScrollBar = yPosItemBG; //sets starting scroll bar Y pos. Will be manipulated for smooth scrolling later.
        }

        public void MaxItemsInMenu(int number)
        {
            MaxItemsOnScreen = number;
            maxItem = number - 1;
        }

        public void ResetIndexPosition()
        {
            SelectedIndex = 0;
            minItem = 0;
            MaxItemsInMenu(MaxItemsOnScreen);
        }

        public void SetIndexPosition(int indexPosition)
        {
            SelectedIndex = indexPosition;

            if (SelectedIndex >= MaxItemsOnScreen)
            {
                //int possibleMin = SelectedIndex - MaxItemsOnScreen;
                minItem = SelectedIndex - MaxItemsOnScreen;
                maxItem = SelectedIndex;
            }
            else
            {
                minItem = 0;
                maxItem = MaxItemsOnScreen - 1;
            }
        }

        public void AddMenuItem(UIMenuItem item)
        {
            _itemList.Add(item);
        }

        public void BindItemToSubmenu(UIMenu submenu, UIMenuItem itemToBindTo)
        {
            submenu.ParentMenu = this;
            submenu.ParentItem = itemToBindTo;
            /*if (Binded.ContainsKey(itemToBindTo))
                Binded[itemToBindTo] = submenu;
            else
                Binded.Add(itemToBindTo, submenu);*/
            _bindedList.Add(new BindedItem { BindedSubmenu = submenu, BindedItemToSubmenu = itemToBindTo });
        }

        public List<UIMenuItem> UIMenuItemList
        {
            get { return _itemList; }
            set { _itemList = value; }
        }

        public virtual void Draw()
        {
            if (IsVisible)
            {
                DisplayMenu();
                DisableControls();
                DrawScrollBar();
                ManageCurrentIndex();
                /*if (SelectedItem is UIMenuListItem)
                {
                    SelectedItem.ChangeListIndex();
                }*/
                //UI.ShowSubtitle("selectedIndex: " + SelectedIndex + ", minItem: " + minItem + ", maxItem: " + maxItem); //Debug

                if (/*BindingMenuItem != null && NextMenu != null*/ _bindedList.Count > 0)
                {
                    if (JustPressedAccept() && /*BindingMenuItem == SelectedItem*/ _bindedList.Any(bind => bind.BindedItemToSubmenu == SelectedItem))
                    {
                        IsVisible = false;

                        foreach (var bind in _bindedList.Where(bind => bind.BindedItemToSubmenu == SelectedItem))
                        {
                            bind.BindedSubmenu.IsVisible = true;
                            //bind.BindedSubmenu.AcceptPressed = false;
                            bind.BindedSubmenu.InputTimer = DateTime.Now.AddMilliseconds(350);
                        }

                        InputTimer = DateTime.Now.AddMilliseconds(350);
                        //return;
                    }
                }

                if (JustPressedCancel())
                {
                    IsVisible = false;

                    if (ParentMenu != null)
                    {
                        ParentMenu.IsVisible = true;
                        //ParentMenu.CancelPressed = false;
                        ParentMenu.InputTimer = DateTime.Now.AddMilliseconds(350);
                    }

                    //CancelPressed = false;
                    InputTimer = DateTime.Now.AddMilliseconds(350);
                    //return;
                }

                if (UseEventBasedControls)
                {
                    if (JustPressedAccept())
                    {
                        ItemSelect(SelectedItem, SelectedIndex);
                        InputTimer = DateTime.Now.AddMilliseconds(InputWait);
                        //AcceptPressed = false;
                    }

                    if (JustPressedLeft())
                    {
                        ItemLeftRight(SelectedItem, SelectedIndex, true);
                        InputTimer = DateTime.Now.AddMilliseconds(InputWait);
                    }

                    if (JustPressedRight())
                    {
                        ItemLeftRight(SelectedItem, SelectedIndex, false);
                        InputTimer = DateTime.Now.AddMilliseconds(InputWait);
                    }
                }
            }
        }


        protected void DisplayMenu()
        {
            DrawCustomText(Title, TitleFontSize, GTA.Font.HouseScript, TitleColor.R, TitleColor.G, TitleColor.B, TitleColor.A, xPosBG, yPosTitleText, TextJustification.Center); //Draw title text
            DrawRectangle(xPosBG, yPosTitleBG, MenuBGWidth, TitleBGHeight, TitleBackgroundColor.R, TitleBackgroundColor.G, TitleBackgroundColor.B, TitleBackgroundColor.A); //Draw main rectangle
            DrawRectangle(xPosBG, yPosUnderline, MenuBGWidth, UnderlineHeight, TitleUnderlineColor.R, TitleUnderlineColor.G, TitleUnderlineColor.B, TitleUnderlineColor.A); //Draw rectangle as underline of title

            foreach (UIMenuItem item in _itemList)
            {
                bool ScrollOrNotDecision = (UseScroll && _itemList.IndexOf(item) >= minItem && _itemList.IndexOf(item) <= maxItem) || !UseScroll;
                if (ScrollOrNotDecision)
                {
                    YPosBasedOnScroll = UseScroll && _itemList.Count > MaxItemsOnScreen ? CalculatePosition(_itemList.IndexOf(item), minItem, maxItem, 0, MaxItemsOnScreen - 1) : _itemList.IndexOf(item);
                    YPosDescBasedOnScroll = UseScroll && _itemList.Count > MaxItemsOnScreen ? MaxItemsOnScreen : _itemList.Count;

                    if (_itemList.IndexOf(item) == SelectedIndex)
                    {
                        DrawCustomText(item.Text, ItemTextFontSize, ItemTextFontType, HighlightedItemTextColor.R, HighlightedItemTextColor.G, HighlightedItemTextColor.B, HighlightedItemTextColor.A, xPosItemText, yPosItem + YPosBasedOnScroll * posMultiplier); //Draw highlighted item text

                        if (item.Value != null)
                        { DrawCustomText(Convert.ToString(item.Value), ItemTextFontSize, ItemTextFontType, HighlightedItemTextColor.R, HighlightedItemTextColor.G, HighlightedItemTextColor.B, HighlightedItemTextColor.A, xPosItemValue, yPosItem + YPosBasedOnScroll * posMultiplier, TextJustification.Right); } //Draw highlighted item value

                        DrawRectangle(xPosBG, yPosItemBG + YPosBasedOnScroll * posMultiplier, MenuBGWidth, heightItemBG, HighlightedBoxColor.R, HighlightedBoxColor.G, HighlightedBoxColor.B, HighlightedBoxColor.A); //Draw rectangle over highlighted text

                        if (item.Description != null)
                        {
                            /*foreach (string desc in item.DescriptionTexts)
                            {
                                DrawCustomText(desc, ItemTextFontSize, ItemTextFontType, DescriptionTextColor.R, DescriptionTextColor.G, DescriptionTextColor.B, DescriptionTextColor.A, xPosItemText, yPosItem + (item.DescriptionTexts.IndexOf(desc) + YPosDescBasedOnScroll) * posMultiplier, TextJustification.Left, false); // Draw description text at bottom of menu
                                DrawRectangle(xPosBG, yPosItemBG + (item.DescriptionTexts.IndexOf(desc) + YPosDescBasedOnScroll) * posMultiplier, MenuBGWidth, heightItemBG, DescriptionBoxColor.R, DescriptionBoxColor.G, DescriptionBoxColor.B, DescriptionBoxColor.A); //Draw rectangle over description text at bottom of the list.
                            }*/

                            DrawCustomText(item.Description, ItemTextFontSize, ItemTextFontType, DescriptionTextColor.R, DescriptionTextColor.G, DescriptionTextColor.B, DescriptionTextColor.A, xPosItemText, yPosItem + YPosDescBasedOnScroll * posMultiplier, TextJustification.Left, true); // Draw description text at bottom of menu
                            float numLines = item.DescriptionWidth / (boxWidth - 10);
                            for (int l = 0; l < (int)Math.Ceiling(numLines); l++)
                            {
                                DrawRectangle(xPosBG, yPosItemBG + (l + YPosDescBasedOnScroll) * posMultiplier, MenuBGWidth, heightItemBG, DescriptionBoxColor.R, DescriptionBoxColor.G, DescriptionBoxColor.B, DescriptionBoxColor.A); //Draw rectangle over description text at bottom of the list.
                            }
                            //UI.ShowSubtitle(numLines.ToString());
                        }

                        SelectedItem = item;
                    }
                    else
                    {
                        DrawCustomText(item.Text, ItemTextFontSize, ItemTextFontType, DefaultTextColor.R, DefaultTextColor.G, DefaultTextColor.B, DefaultTextColor.A, xPosItemText, yPosItem + YPosBasedOnScroll * posMultiplier); //Draw item text

                        if (item.Value != null)
                        { DrawCustomText(Convert.ToString(item.Value), ItemTextFontSize, ItemTextFontType, DefaultTextColor.R, DefaultTextColor.G, DefaultTextColor.B, DefaultTextColor.A, xPosItemValue, yPosItem + YPosBasedOnScroll * posMultiplier, TextJustification.Right); } //Draw item value

                        DrawRectangle(xPosBG, yPosItemBG + YPosBasedOnScroll * posMultiplier, MenuBGWidth, heightItemBG, DefaultBoxColor.R, DefaultBoxColor.G, DefaultBoxColor.B, DefaultBoxColor.A); //Draw background rectangles around all items.
                    }
                }
            }

            //DevMenuPositioner();
        }

        void DevMenuPositioner()
        {
            if (Game.IsKeyPressed(Keys.NumPad6))
            {
                ItemTextFontSize = (float)Math.Round(ItemTextFontSize + 0.001, 3);
            }
            if (Game.IsKeyPressed(Keys.NumPad4))
            {
                ItemTextFontSize = (float)Math.Round(ItemTextFontSize - 0.001, 3);
            }
            if (Game.IsKeyPressed(Keys.NumPad8))
            {
                heightItemBG = (float)Math.Round(heightItemBG + 0.001, 3);
            }
            if (Game.IsKeyPressed(Keys.NumPad2))
            {
                heightItemBG = (float)Math.Round(heightItemBG - 0.001, 3);
            }
            if (Game.IsKeyPressed(Keys.NumPad9))
            {
                posMultiplier = (float)Math.Round(posMultiplier + 0.001, 3);
            }
            if (Game.IsKeyPressed(Keys.NumPad7))
            {
                posMultiplier = (float)Math.Round(posMultiplier - 0.001, 3);
            }
            if (Game.IsKeyPressed(Keys.NumPad3))
            {
                yTextOffset = (float)Math.Round(yTextOffset + 0.001, 3);
            }
            if (Game.IsKeyPressed(Keys.NumPad1))
            {
                yTextOffset = (float)Math.Round(yTextOffset - 0.001, 3);
            }
            CalculateMenuPositioning();
            UI.ShowSubtitle("ItemTextFontSize: " + ItemTextFontSize + ", heightItemBG: " + heightItemBG + ", posMultiplier: " + posMultiplier + ", yTextOffset: " + yTextOffset);
        }

        protected void DrawScrollBar()
        {
            if (UseScroll && _itemList.Count > MaxItemsOnScreen)
            {
                YPosSmoothScrollBar = CalculateSmoothPosition(YPosSmoothScrollBar, CalculateScroll(SelectedIndex, 0, _itemList.Count - 1, yPosItemBG, yPosItemBG + (MaxItemsOnScreen - 1) * posMultiplier), 0.0005f, yPosItemBG, yPosItemBG + (MaxItemsOnScreen - 1) * posMultiplier);
                DrawRectangle(xPosScrollBar, YPosSmoothScrollBar, ScrollBarWidth, heightItemBG, TitleUnderlineColor.R, TitleUnderlineColor.G, TitleUnderlineColor.B, TitleUnderlineColor.A);

                //DrawRectangle(xPosScrollBar, CalculateScroll(SelectedIndex, 0, _itemList.Count - 1, yPosItemBG, yPosItemBG + (MaxItemsOnScreen - 1) * posMultiplier), ScrollBarWidth, heightItemBG, TitleUnderlineColor.R, TitleUnderlineColor.G, TitleUnderlineColor.B, TitleUnderlineColor.A);
            }
        }

        int CalculatePosition(int input, int inputMin, int inputMax, int outputMin, int outputMax)
        {
            //http://stackoverflow.com/questions/22083199/method-for-calculating-a-value-relative-to-min-max-values
            //Making sure bounderies arent broken...
            if (input > inputMax)
            {
                input = inputMax;
            }
            if (input < inputMin)
            {
                input = inputMin;
            }
            //Return value in relation to min og max

            double position = (double)(input - inputMin) / (inputMax - inputMin);

            int relativeValue = (int)(position * (outputMax - outputMin)) + outputMin;

            return relativeValue;
        }

        float CalculateScroll(float input, float inputMin, float inputMax, float outputMin, float outputMax)
        {
            //http://stackoverflow.com/questions/22083199/method-for-calculating-a-value-relative-to-min-max-values
            //Making sure bounderies arent broken...
            if (input > inputMax)
            {
                input = inputMax;
            }
            if (input < inputMin)
            {
                input = inputMin;
            }
            //Return value in relation to min og max

            double position = (double)(input - inputMin) / (inputMax - inputMin);

            float relativeValue = (float)(position * (outputMax - outputMin)) + outputMin;

            return relativeValue;
        }

        float CalculateSmoothPosition(float currentPosition, float desiredPosition, float step, float min, float max)
        {
            if (currentPosition == desiredPosition) return currentPosition;

            if (currentPosition < desiredPosition)
            {
                //currentPosition += (desiredPosition - currentPosition) * 0.1f;
                currentPosition += (desiredPosition - currentPosition) * 5f * Game.LastFrameTime;
                if (currentPosition > max)
                {
                    currentPosition = max;
                }
                return currentPosition;
            }
            else if (currentPosition > desiredPosition)
            {
                //currentPosition -= (currentPosition - desiredPosition) * 0.1f;
                currentPosition -= (currentPosition - desiredPosition) * 5f * Game.LastFrameTime;
                if (currentPosition < min)
                {
                    currentPosition = min;
                }
                return currentPosition;
            }
            return currentPosition;
        }

        enum TextJustification
        {
            Center = 0,
            Left,
            Right //requires SET_TEXT_WRAP
        }

        void DrawCustomText(string Message, float FontSize, GTA.Font FontType, int Red, int Green, int Blue, int Alpha, float XPos, float YPos, TextJustification justifyType = TextJustification.Left, bool ForceTextWrap = false)
        {
            Function.Call(Hash._SET_TEXT_ENTRY, "jamyfafi"); //Required, don't change this! AKA BEGIN_TEXT_COMMAND_DISPLAY_TEXT
            Function.Call(Hash.SET_TEXT_SCALE, 1.0f, FontSize);
            Function.Call(Hash.SET_TEXT_FONT, (int)FontType);
            Function.Call(Hash.SET_TEXT_COLOUR, Red, Green, Blue, Alpha);
            //Function.Call(Hash.SET_TEXT_DROPSHADOW, 0, 0, 0, 0, 0);
            Function.Call(Hash.SET_TEXT_JUSTIFICATION, (int)justifyType);
            if (justifyType == TextJustification.Right || ForceTextWrap)
            {
                Function.Call(Hash.SET_TEXT_WRAP, xPosItemText, xPosItemValue);
            }

            //Function.Call(Hash._ADD_TEXT_COMPONENT_STRING, Message);
            StringHelper.AddLongString(Message);

            Function.Call(Hash._DRAW_TEXT, XPos, YPos); //AKA END_TEXT_COMMAND_DISPLAY_TEXT
        }

        void DrawRectangle(float BgXpos, float BgYpos, float BgWidth, float BgHeight, int bgR, int bgG, int bgB, int bgA)
        {
            Function.Call(Hash.DRAW_RECT, BgXpos, BgYpos, BgWidth, BgHeight, bgR, bgG, bgB, bgA);
        }

        protected virtual void ManageCurrentIndex()
        {
            if (JustPressedUp())
            {
                if (SelectedIndex > 0 && SelectedIndex <= _itemList.Count - 1)
                {
                    SelectedIndex--;
                    if (SelectedIndex < minItem && minItem > 0)
                    {
                        minItem--;
                        maxItem--;
                    }
                }
                else if (SelectedIndex == 0)
                {
                    SelectedIndex = _itemList.Count - 1;
                    minItem = _itemList.Count - MaxItemsOnScreen;
                    maxItem = _itemList.Count - 1;
                }
                else
                {
                    SelectedIndex = _itemList.Count - 1;
                    minItem = _itemList.Count - MaxItemsOnScreen;
                    maxItem = _itemList.Count - 1;
                }

                if (IsHoldingSpeedupControl())
                {
                    InputTimer = DateTime.Now.AddMilliseconds(20);
                }
                else
                {
                    InputTimer = DateTime.Now.AddMilliseconds(InputWait);
                }
            }

            if (JustPressedDown())
            {
                if (SelectedIndex >= 0 && SelectedIndex < _itemList.Count - 1)
                {
                    SelectedIndex++;
                    if (SelectedIndex >= maxItem + 1)
                    {
                        minItem++;
                        maxItem++;
                    }
                }
                else if (SelectedIndex == _itemList.Count - 1)
                {
                    SelectedIndex = 0;
                    minItem = 0;
                    maxItem = MaxItemsOnScreen - 1;
                }
                else
                {
                    SelectedIndex = 0;
                    minItem = 0;
                    maxItem = MaxItemsOnScreen - 1;
                }

                if (IsHoldingSpeedupControl())
                {
                    InputTimer = DateTime.Now.AddMilliseconds(20);
                }
                else
                {
                    InputTimer = DateTime.Now.AddMilliseconds(InputWait);
                }
            }
        }

        List<Control> ControlsToEnable = new List<Control>
            {
                /*Control.FrontendAccept,
                Control.FrontendAxisX,
                Control.FrontendAxisY,
                Control.FrontendDown,
                Control.FrontendUp,
                Control.FrontendLeft,
                Control.FrontendRight,
                Control.FrontendCancel,
                Control.FrontendSelect,
                Control.CharacterWheel,
                Control.CursorScrollDown,
                Control.CursorScrollUp,
                Control.CursorX,
                Control.CursorY,*/
                Control.MoveUpDown,
                Control.MoveLeftRight,
                Control.Sprint,
                Control.Jump,
                Control.Enter,
                Control.VehicleExit,
                Control.VehicleAccelerate,
                Control.VehicleBrake,
                Control.VehicleMoveLeftRight,
                Control.VehicleFlyYawLeft,
                Control.FlyLeftRight,
                Control.FlyUpDown,
                Control.VehicleFlyYawRight,
                Control.VehicleHandbrake,
                /*Control.VehicleRadioWheel,
                Control.VehicleRoof,
                Control.VehicleHeadlight,
                Control.VehicleCinCam,
                Control.Phone,
                Control.MeleeAttack1,
                Control.MeleeAttack2,
                Control.Attack,
                Control.Attack2*/
                Control.LookUpDown,
                Control.LookLeftRight
            };

        protected void DisableControls()
        {
            Game.DisableAllControlsThisFrame(2);

            foreach (var con in ControlsToEnable)
            {
                Game.EnableControlThisFrame(2, con);



            }
        }

        bool IsGamepad()
        {
            return Game.CurrentInputMode == InputMode.GamePad;
        }

        public bool JustPressedUp()
        {
            if ((IsGamepad() && Game.IsControlPressed(2, Control.PhoneUp)) || Game.IsKeyPressed(Keys.NumPad8) || Game.IsKeyPressed(Keys.Up))
            {
                if (InputTimer < DateTime.Now)
                {
                    Game.PlaySound(AUDIO_UPDOWN, AUDIO_LIBRARY);
                    return true;
                }
            }
            return false;
        }

        public bool JustPressedDown()
        {
            if ((IsGamepad() && Game.IsControlPressed(2, Control.PhoneDown)) || Game.IsKeyPressed(Keys.NumPad2) || Game.IsKeyPressed(Keys.Down))
            {
                if (InputTimer < DateTime.Now)
                {
                    Game.PlaySound(AUDIO_UPDOWN, AUDIO_LIBRARY);
                    return true;
                }
            }
            return false;
        }

        public bool JustPressedLeft()
        {
            if ((IsGamepad() && Game.IsControlPressed(2, Control.PhoneLeft)) || Game.IsKeyPressed(Keys.NumPad4) || Game.IsKeyPressed(Keys.Left))
            {
                if (InputTimer < DateTime.Now)
                {
                    Game.PlaySound(AUDIO_LEFTRIGHT, AUDIO_LIBRARY);
                    return true;
                }
            }
            return false;
        }

        public bool JustPressedRight()
        {
            if ((IsGamepad() && Game.IsControlPressed(2, Control.PhoneRight)) || Game.IsKeyPressed(Keys.NumPad6) || Game.IsKeyPressed(Keys.Right))
            {
                if (InputTimer < DateTime.Now)
                {
                    Game.PlaySound(AUDIO_LEFTRIGHT, AUDIO_LIBRARY);
                    return true;
                }
            }
            return false;
        }

        /*public bool JustPressedAccept()
        {
            if ((IsGamepad() && Game.IsControlJustPressed(2, Control.PhoneSelect)) || AcceptPressed)
            {
                Game.PlaySound(AUDIO_SELECT, AUDIO_LIBRARY);
                //AcceptPressed = false;
                return true;
            }
            return false;
        }

        public bool JustPressedCancel()
        {
            if ((IsGamepad() && Game.IsControlJustPressed(2, Control.PhoneCancel)) || CancelPressed)
            {
                Game.PlaySound(AUDIO_BACK, AUDIO_LIBRARY);
                //CancelPressed = false;
                return true;
            }
            return false;
        }*/

        public bool JustPressedAccept()
        {
            if (Game.IsControlPressed(2, Control.PhoneSelect) || Game.IsKeyPressed(Keys.NumPad5) || Game.IsKeyPressed(Keys.Enter))
            {
                if (InputTimer < DateTime.Now)
                {
                    Game.PlaySound(AUDIO_SELECT, AUDIO_LIBRARY);
                    //InputTimer = Game.GameTime + 350;
                    return true;
                }
            }
            return false;
        }

        public bool JustPressedCancel()
        {
            if (Game.IsControlPressed(2, Control.PhoneCancel) || Game.IsKeyPressed(Keys.NumPad0) || Game.IsKeyPressed(Keys.Back))
            {
                if (InputTimer < DateTime.Now)
                {
                    Game.PlaySound(AUDIO_BACK, AUDIO_LIBRARY);
                    //InputTimer = Game.GameTime + InputWait;
                    return true;
                }
            }
            return false;
        }

        bool IsHoldingSpeedupControl()
        {
            if (IsGamepad())
            {
                return Game.IsControlPressed(2, Control.VehicleHandbrake);
            }
            else
            {
                return Game.IsKeyPressed(Keys.ShiftKey);
            }
        }

        public void SetInputWait(int ms = 350)
        {
            InputTimer = DateTime.Now.AddMilliseconds(ms);
        }

        public bool ControlBoolValue(UIMenuItem item, bool boolToControl)
        {
            if (IsVisible && SelectedItem == item)
            {
                //if (JustPressedAccept())
                //{
                boolToControl = !boolToControl;
                item.Value = boolToControl;
                //InputTimer = DateTime.Now.AddMilliseconds(InputWait);
                return boolToControl;
                //}
            }
            item.Value = boolToControl;
            return boolToControl;
        }

        public bool ControlBoolValue_NoEvent(UIMenuItem item, bool boolToControl)
        {
            item.Value = boolToControl;

            if (IsVisible && SelectedItem == item)
            {
                if (JustPressedAccept())
                {
                    boolToControl = !boolToControl;
                    item.Value = boolToControl;
                    InputTimer = DateTime.Now.AddMilliseconds(InputWait);
                    return boolToControl;
                }
            }
            return boolToControl;
        }

        public float ControlFloatValue(UIMenuItem item, bool left, float numberToControl, float incrementValue, float incrementValueFast, int decimals = 2, bool limit = false, float min = 0f, float max = 1f)
        {
            if (IsVisible && SelectedItem == item)
            {
                if (left)
                {
                    if (IsHoldingSpeedupControl())
                    {
                        numberToControl -= incrementValueFast;
                    }
                    else
                    {
                        numberToControl -= incrementValue;
                    }
                }
                if (!left)
                {
                    if (IsHoldingSpeedupControl())
                    {
                        numberToControl += incrementValueFast;
                    }
                    else
                    {
                        numberToControl += incrementValue;
                    }
                }
                if (limit)
                {
                    if (numberToControl < min)
                    {
                        numberToControl = min;
                    }
                    if (numberToControl > max)
                    {
                        numberToControl = max;
                    }
                }

                item.Value = "< " + numberToControl + " >";

                //InputTimer = DateTime.Now.AddMilliseconds(InputWait);

                return (float)Math.Round(numberToControl, decimals);
            }
            item.Value = "< " + numberToControl + " >";
            return numberToControl;
        }

        public float ControlFloatValue_NoEvent(UIMenuItem item, float numberToControl, float incrementValue, float incrementValueFast, int decimals = 2, bool limit = false, float min = 0f, float max = 1f)
        {
            item.Value = "< " + numberToControl + " >";

            if (IsVisible && SelectedItem == item)
            {
                if (JustPressedLeft())
                {
                    if (IsHoldingSpeedupControl())
                    {
                        numberToControl -= incrementValueFast;
                    }
                    else
                    {
                        numberToControl -= incrementValue;
                    }

                    if (limit)
                    {
                        if (numberToControl < min)
                        {
                            numberToControl = min;
                        }
                        if (numberToControl > max)
                        {
                            numberToControl = max;
                        }
                    }

                    item.Value = "< " + numberToControl + " >";

                    InputTimer = DateTime.Now.AddMilliseconds(InputWait);

                    return (float)Math.Round(numberToControl, decimals);
                }
                if (JustPressedRight())
                {
                    if (IsHoldingSpeedupControl())
                    {
                        numberToControl += incrementValueFast;
                    }
                    else
                    {
                        numberToControl += incrementValue;
                    }

                    if (limit)
                    {
                        if (numberToControl < min)
                        {
                            numberToControl = min;
                        }
                        if (numberToControl > max)
                        {
                            numberToControl = max;
                        }
                    }

                    item.Value = "< " + numberToControl + " >";

                    InputTimer = DateTime.Now.AddMilliseconds(InputWait);

                    return (float)Math.Round(numberToControl, decimals);
                }
            }
            return numberToControl;
        }

        public int ControlIntValue(UIMenuItem item, bool left, int numberToControl, int incrementValue, int incrementValueFast, bool limit = false, int min = 0, int max = 100)
        {
            if (IsVisible && SelectedItem == item)
            {
                if (left)
                {
                    if (IsHoldingSpeedupControl())
                    {
                        numberToControl -= incrementValueFast;
                    }
                    else
                    {
                        numberToControl -= incrementValue;
                    }
                }
                if (!left)
                {
                    if (IsHoldingSpeedupControl())
                    {
                        numberToControl += incrementValueFast;
                    }
                    else
                    {
                        numberToControl += incrementValue;
                    }
                }
                if (limit)
                {
                    if (numberToControl < min)
                    {
                        numberToControl = min;
                    }
                    else if (numberToControl > max)
                    {
                        numberToControl = max;
                    }
                }

                item.Value = "< " + numberToControl + " >";

                //InputTimer = DateTime.Now.AddMilliseconds(InputWait);

            }
            item.Value = "< " + numberToControl + " >";
            return numberToControl;
        }

        public int ControlIntValue_NoEvent(UIMenuItem item, int numberToControl, int incrementValue, int incrementValueFast, bool limit = false, int min = 0, int max = 100)
        {
            item.Value = "< " + numberToControl + " >";

            if (IsVisible && SelectedItem == item)
            {
                if (JustPressedLeft())
                {
                    if (IsHoldingSpeedupControl())
                    {
                        numberToControl -= incrementValueFast;
                    }
                    else
                    {
                        numberToControl -= incrementValue;
                    }

                    if (limit)
                    {
                        if (numberToControl < min)
                        {
                            numberToControl = min;
                        }
                        if (numberToControl > max)
                        {
                            numberToControl = max;
                        }
                    }

                    item.Value = "< " + numberToControl + " >";

                    InputTimer = DateTime.Now.AddMilliseconds(InputWait);

                    return numberToControl;
                }
                if (JustPressedRight())
                {
                    if (IsHoldingSpeedupControl())
                    {
                        numberToControl += incrementValueFast;
                    }
                    else
                    {
                        numberToControl += incrementValue;
                    }

                    if (limit)
                    {
                        if (numberToControl < min)
                        {
                            numberToControl = min;
                        }
                        if (numberToControl > max)
                        {
                            numberToControl = max;
                        }
                    }

                    item.Value = "< " + numberToControl + " >";

                    InputTimer = DateTime.Now.AddMilliseconds(InputWait);

                    return numberToControl;
                }
            }
            return numberToControl;
        }

        protected virtual void ItemSelect(UIMenuItem selecteditem, int index)
        {
            OnItemSelect?.Invoke(this, selecteditem, index);
        }

        protected virtual void ItemLeftRight(UIMenuItem selecteditem, int index, bool left)
        {
            OnItemLeftRight?.Invoke(this, selecteditem, index, left);
        }
    }

    public class UIMenuDisplayOnly : UIMenu
    {
        public UIMenuDisplayOnly(string text) : base(text)
        {
            base.TitleFontSize = 0.5f;
            base.boxWidth = 400;

            CalculateMenuPositioning();

            MaxItemsInMenu(8);
        }

        public override void Draw()
        {
            if (IsVisible)
            {
                DisplayMenu();
                DisableControls();
                DrawScrollBar();
                //ManageCurrentIndex();
            }
        }

        protected override void ManageCurrentIndex()
        {
            //base.ManageCurrentIndex();
        }

        public void GoToNextItem()
        {
            SelectedIndex++;
            if (SelectedIndex >= maxItem + 1)
            {
                minItem++;
                maxItem++;
            }
        }

        public void GoToFirstItem()
        {
            SelectedIndex = 0;
            minItem = 0;
            maxItem = MaxItemsOnScreen - 1;
        }

        public void GoToPreviousItem()
        {
            SelectedIndex--;
            if (SelectedIndex < minItem && minItem > 0)
            {
                minItem--;
                maxItem--;
            }
        }

        public void GoToLastItem()
        {
            SelectedIndex = _itemList.Count - 1;
            minItem = _itemList.Count - MaxItemsOnScreen;
            maxItem = _itemList.Count - 1;
        }
    }

    public class UIMenuItem
    {
        string _text { get; set; }
        dynamic _value { get; set; }
        string _description { get; set; }
        public List<string> DescriptionTexts;
        public float DescriptionWidth { get; set; }

        public UIMenuItem(string text)
        {
            _text = text;
        }

        public UIMenuItem(string text, dynamic value)
        {
            _text = text;
            _value = value;
        }

        public UIMenuItem(string text, dynamic value, string description)
        {
            _text = text;
            _value = value;
            _description = description;
            //DescriptionTexts = description.SplitOn(90);

            if (_description != null)
            { DescriptionWidth = StringHelper.MeasureStringWidth(_description, GTA.Font.ChaletComprimeCologne, 0.452f); }
        }

        public string Text
        {
            get { return _text; }
            set { _text = value; }
        }

        public dynamic Value
        {
            get { return _value; }
            set { _value = value; }
        }

        public string Description
        {
            get { return _description; }
            set
            {
                //DescriptionTexts = value.SplitOn(90);

                if (value != null)
                { DescriptionWidth = StringHelper.MeasureStringWidth(value, GTA.Font.ChaletComprimeCologne, 0.452f); }

                _description = value;
            }
        }

        public virtual void ChangeListIndex() { }
    }

    public static class StringHelper
    {
        public static void AddLongString(string str)
        {
            const int strLen = 99;
            for (int i = 0; i < str.Length; i += strLen)
            {
                string substr = str.Substring(i, Math.Min(strLen, str.Length - i));
                Function.Call(Hash._ADD_TEXT_COMPONENT_STRING, substr); //ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME
            }
        }

        public static float MeasureStringWidth(string str, GTA.Font font, float fontsize)
        {
            //int screenw = 2560;// Game.ScreenResolution.Width;
            //int screenh = 1440;// Game.ScreenResolution.Height;
            const float height = 1080f;
            float ratio = (float)Game.ScreenResolution.Width / Game.ScreenResolution.Height;
            float width = height * ratio;
            return MeasureStringWidthNoConvert(str, font, fontsize) * width;
        }

        private static float MeasureStringWidthNoConvert(string str, GTA.Font font, float fontsize)
        {
            Function.Call((Hash)0x54CE8AC98E120CAB, "jamyfafi"); //_BEGIN_TEXT_COMMAND_WIDTH
            AddLongString(str);
            Function.Call(Hash.SET_TEXT_FONT, (int)font);
            Function.Call(Hash.SET_TEXT_SCALE, fontsize, fontsize);
            return Function.Call<float>(Hash._0x85F061DA64ED2F67, true); //_END_TEXT_COMMAND_GET_WIDTH //Function.Call<float>((Hash)0x85F061DA64ED2F67, (int)font) * fontsize; //_END_TEXT_COMMAND_GET_WIDTH
        }
    }

    public class UIMenuNumberValueItem : UIMenuItem
    {
        public UIMenuNumberValueItem(string text, dynamic value) : base(text, (object)value)
        {
            this.Text = text;
            this.Value = "< " + value + " >";
        }

        public UIMenuNumberValueItem(string text, dynamic value, string description) : base(text, (object)value, description)
        {
            this.Text = text;
            this.Value = "< " + value + " >";
            this.Description = description;
            //DescriptionTexts = description.SplitOn(90);

            if (description != null)
            { DescriptionWidth = StringHelper.MeasureStringWidth(description, GTA.Font.ChaletComprimeCologne, 0.452f); }
        }
    }

    /*public class UIMenuListItem : UIMenuItem
    {
        public List<dynamic> List { get; set; }
        public int SelectedIndex = 0;

        public UIMenuListItem(string text, dynamic value, string description, List<dynamic> list)
        {
            this.Text = text;
            this.Value = value;
            this.Description = description;
            List = list;
        }

        public override void ChangeListIndex()
        {

        }
    }*/

    class BindedItem
    {
        private UIMenu _menu;
        private UIMenuItem _item;

        public UIMenu BindedSubmenu
        {
            get { return _menu; }
            set { _menu = value; }
        }

        public UIMenuItem BindedItemToSubmenu
        {
            get { return _item; }
            set { _item = value; }
        }
    }

    /*public static class SplitStringByLength
    {
        public static IEnumerable<string> SplitByLength(this string str, int maxLength)
        {
            for (int index = 0; index < str.Length; index += maxLength)
            {
                yield return str.Substring(index, Math.Min(maxLength, str.Length - index));
            }
        }
    }*/

    // using System.Text.RegularExpressions;
    public static class StringExtensions
    {

        /// <summary>Use this function like string.Split but instead of a character to split on, 
        /// use a maximum line width size. This is similar to a Word Wrap where no words will be split.</summary>
        /// Note if the a word is longer than the maxcharactes it will be trimmed from the start.
        /// <param name="initial">The string to parse.</param>
        /// <param name="MaxCharacters">The maximum size.</param>
        /// <remarks>This function will remove some white space at the end of a line, but allow for a blank line.</remarks>
        /// 
        /// <returns>An array of strings.</returns>
        public static List<string> SplitOn(this string initial, int MaxCharacters)
        {

            List<string> lines = new List<string>();

            if (string.IsNullOrEmpty(initial) == false)
            {
                string targetGroup = "Line";
                string pattern = string.Format(@"(?<{0}>.{{1,{1}}})(?:\W|$)", targetGroup, MaxCharacters);

                lines = Regex.Matches(initial, pattern, RegexOptions.Multiline | RegexOptions.CultureInvariant)
                             .OfType<Match>()
                             .Select(mt => mt.Groups[targetGroup].Value)
                             .ToList();
            }
            return lines;
        }
    }
}
