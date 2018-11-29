using ClassicUO.Game.GameObjects;
using ClassicUO.Game.Gumps.Controls;
using ClassicUO.Input;
using ClassicUO.Network;
using ClassicUO.Renderer;
using ClassicUO.Utility;
using ClassicUO.Utility.Logging;
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
        private TextBox m_Title, m_Author;
        public ushort BookPageCount { get; internal set; }
        public string Title { get; internal set; }
        public string Author { get; internal set; }
        public bool IsNewBookD4 { get; internal set; }
        public Dictionary<int, List<string>> BookPages { get; internal set; }

        public Dictionary<int, List<TextBox>> Lines = new Dictionary<int, List<TextBox>>();
        public bool IsBookEditable { get; internal set; }

        public bool IsDirty => m_Title?.Text != Title || m_Author?.Text != Author;
        public bool IsContentsDirty => m_pages.Any( t => t.Item1 != t.Item2.Text );
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

            AddChildren( m_Title = new TextBox( 5, 30, 0, 155, false ) { X = 40, Y = 40, Text = Title ?? "", Height = 25, Width = 155 } ,1);
            AddChildren( new Label( "by", true, 1 ) { X = 45, Y = 110 },1);

            AddChildren( m_Author = new TextBox( 5, 30, 0, 155, false ) { X = 45, Y = 130, Height = 25, Width = 155, Text = Author ?? "" } ,1);

            for ( int k = 1; k < BookPageCount; k++ )
            {
                List<string> p = null;
                if(k < BookPages.Count)
                    p = BookPages[k];
                int x = 38;
                int y = 30;
                if ( k % 2 == 1 )
                {
                    x = 223;
                    //right hand page
                }
                int page = k + 1;
                if ( page % 2 == 1 )
                    page += 1;
                page /= 2;
                TextBox tbox;
                AddChildren( tbox = new TextBox( 1, 0, 0, 155, false ) { X = x, Y = y, Height = 170, Width = 155, IsEditable = true, Text = "", MultiLineInputAllowed = true , MaxLines = 8}, page );

                for ( int i = 0; i < 8; i++ )
                {
                    var txt = ( p != null && p.Count > i ? p[i] : "" );

                    if ( i < 7 && !txt.EndsWith( "\n" ) )
                        txt += "\n";
                    tbox.SetText( txt, true );
                }
                m_pages.Add( (tbox.Text, tbox) );
                AddChildren( new Label( k.ToString(), true, 1 ) { X = x + 80, Y = 200 }, page );

                
            }
            SetActivePage( 1 );
        }
        private List<(string,TextBox)> m_pages = new List<(string, TextBox)> ();
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
        public override void OnButtonClick( int buttonID )
        {
            switch ( (Buttons)buttonID )
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
        protected override void OnInitialize()
        {
            
            base.OnInitialize();
            BuildGump();
            Debug = true;
        }
        protected override void CloseWithRightClick()
        {
            
            if ( IsDirty )
            {
                //TODO send book update
                //if title/author send back d4 or 93
                // if page  content cchanged send 0x66
                if ( IsNewBookD4 )
                {
                    NetClient.Socket.Send( new PBookHeaderNew( LocalSerial, m_Title.Text, m_Author.Text, BookPages.Count ) );
                }
                else
                {
                    NetClient.Socket.Send( new PBookHeader( LocalSerial, m_Title.Text, m_Author.Text, BookPages.Count ) );
                }

            }
            if ( IsContentsDirty )
            {
                NetClient.Socket.Send( new PBookData( LocalSerial, m_pages, IsNewBookD4 ) );
            }
            base.CloseWithRightClick();
        }



        public sealed class PBookHeaderNew : PacketWriter
        {
           
            public PBookHeaderNew( Serial serial, string title,string author,int pagecount ) : base( 0xD4 )
            {
                byte[] titleBuffer = Encoding.UTF8.GetBytes( title );
                byte[] authorBuffer = Encoding.UTF8.GetBytes( author );
                EnsureSize( 15 + titleBuffer.Length + authorBuffer.Length );
                WriteUInt( serial );
                WriteByte( pagecount > 0 ? (byte)1 : (byte)0 );
                WriteByte( pagecount > 0 ? (byte)1 : (byte)0 );
                WriteUShort( (ushort)pagecount );

                WriteUShort( (ushort) (titleBuffer.Length + 1) );
                WriteBytes( titleBuffer, 0, titleBuffer.Length );
                WriteByte( 0 );
                WriteUShort( (ushort)(authorBuffer.Length + 1) );
                WriteBytes( authorBuffer, 0, authorBuffer.Length );
                WriteByte( 0 );
            }

           
        }
        public sealed class PBookHeader : PacketWriter
        {
            public PBookHeader( Serial serial, string title, string author, int pagecount ) : base( 0x93 )
            {
               
                EnsureSize( 15 + 60 + 30 );
                WriteUInt( serial );
                WriteByte( pagecount > 0 ? (byte)1 : (byte)0 );
                WriteByte( pagecount > 0 ? (byte)1 : (byte)0 );

                WriteUShort( (ushort)pagecount );

                WriteASCII( title, 60 );
                WriteASCII( author, 30 );
            }
        }
        public sealed class PBookData : PacketWriter
        {
            public PBookData( Serial serial, List<(string, TextBox)> data,bool IsNewFormat ) : base( 0x66 )
            {
                EnsureSize( 256 );

                WriteUInt( serial );
                WriteUShort( (ushort)data.Count );
                for(int i= 0;i < data.Count;i++ )
                {
                    WriteUShort( (ushort)(i+1) );
                    var splits = data[i].Item2.Text.Split( '\n' );
                    WriteUShort( (ushort)splits.Length );
                    if(splits.Length > 8 )
                    {
                        //Broke the book
                        //TODO move extra lines to next page
                        Log.Message( LogTypes.Error, $"Book page {i} split into too many lines: {splits.Length} Additional lines will be lost" );
                    }
                    for ( int j = 0; j < splits.Length && j < 8; j++ )
                    {
                        //each line should be < 80 chars long
                        if ( IsNewFormat )
                        {
                            byte[] buf = Encoding.UTF8.GetBytes( splits[j] );
                            WriteUShort( (ushort)(buf.Length + 1) );
                            WriteBytes( buf, 0, buf.Length );
                            WriteByte( 0 );

                        }
                        else
                        {
                            WriteUShort( (ushort)( splits[j].Length + 1 ) );
                            WriteASCII( splits[j] );
                            WriteByte( 0 );
                        }
                    }


                }

            }
        }
        private enum Buttons
        {
            Closing = 0,
            Forward = 1,
            Backwards = 2
        }
    }
}
