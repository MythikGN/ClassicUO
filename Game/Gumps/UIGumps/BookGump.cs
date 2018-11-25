using ClassicUO.Game.GameObjects;
using ClassicUO.Game.Gumps.Controls;
using ClassicUO.Input;
using ClassicUO.Renderer;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassicUO.Game.Gumps.UIGumps
{
    public class BookGump : Gump
    {
        private int m_CurrentPage = 1;
        public BookGump( Item book ) : base( book.Serial, 0 )
        {
            CanMove = true;
            AcceptMouseInput = true;
            
        }

        private void BuildGump()
        {
            AddChildren( new GumpPic( 0, 0, 0x1FE, 0 )
            {
                CanMove = true
            } );

            Button forward, backwards;
            AddChildren(backwards = new Button( (int)Buttons.Backwards, 0x1FF, 0x1FF, 0x1FF )
            {
                ButtonAction = ButtonAction.Activate,
            } );

            AddChildren(forward =  new Button( (int)Buttons.Forward, 0x200, 0x200, 0x200 )
            {
                X = 356,
                ButtonAction = ButtonAction.Activate
            } );
            forward.MouseClick += ( sender,e ) => {
                if ( e.Button == MouseButton.Left && sender is GumpControl ctrl ) SetActivePage(ActivePage + 1 );
            };
            backwards.MouseClick += ( sender, e ) => {
                if ( e.Button == MouseButton.Left && sender is GumpControl ctrl ) SetActivePage( ActivePage - 1 );
            };
            //public TextEntry(AControl parent, int x, int y, int width, int height, int hue, int entryID, int maxCharCount, string text)

            AddChildren( m_Title = new TextBox( 5, 30, 0, 155, false ) { X = 40, Y = 40, Text = Title ?? "", Height = 25, Width = 155 } ,1);
            AddChildren( new Label( "by", true, 1 ) { X = 45, Y = 110 },1);

            AddChildren( m_Author = new TextBox( 5, 30, 0, 155, false ) { X = 45, Y = 130, Height = 25, Width = 155, Text = Author ?? "" } ,1);

            int cnt = 0;
            int pager = 1;

            foreach(var p in BookPages)
            {
                int x = 38;
                int y = 30;
                if ( p.Key % 2 == 1)
                {
                    x = 223;
                    //right hand page
                }
                int page = p.Key;
                page += 1;
                if ( page % 2 == 1 )
                    page += 1;
                page /= 2;
                for(int i = 0;i < 8;i++ )
                {
                    AddChildren( new TextBox( 1, 30, 0, 155, false ) { X = x, Y = y, Height = 22, Width = 155, IsEditable = true, Text = p.Value.Count > i ? p.Value[i] : "" },page );
                    y += 22;
                }
                AddChildren( new Label( p.Key.ToString(), true, 1 ) { X = x + 80, Y = 200 }, page );

            }
            SetActivePage( 1 );
        }
        private int MaxPage => BookPageCount /2;

        public override bool Draw( SpriteBatchUI spriteBatch, Point position, Vector3? hue = null )
        {
            return base.Draw( spriteBatch, position, hue );
        }

        private void SetActivePage( int page )
        {
            if ( page < 1 )
                page = 1;
            else if ( page > MaxPage )
                page = MaxPage;
            ActivePage = page;
        }

        protected override void OnInitialize()
        {
            BuildGump();
            base.OnInitialize();
        }

        public override void OnButtonClick( int buttonID )
        {
            switch((Buttons)buttonID )
            {
                case Buttons.Backwards:
                    return;
                    break;
                case Buttons.Forward:
                    return;
                    break;
            }
            base.OnButtonClick( buttonID );
        }

        protected override void OnClosing()
        {
            base.OnClosing();
            if ( IsDirty )
            {
                //TODO send book update
                //if title/author send back d4 or 93
                // if page  content cchanged send 0x66
            }
        }
        private enum Buttons
        {
            Forward = 1,
            Backwards = 2
        }

        private TextBox m_Title, m_Author, m_PageOne;
        public ushort BookPageCount { get; internal set; }
        public string Title { get; internal set; }
        public string Author { get; internal set; }
        public bool IsNewBookD4 { get; internal set; }
        public Dictionary<int, List<string>> BookPages { get; internal set; }

        public Dictionary<int, List<TextBox>> Lines = new Dictionary<int, List<TextBox>>();
        public bool IsBookEditable { get; internal set; }

        public bool IsDirty => m_Title.Text != Title || m_Author.Text != Author;
    }
}
