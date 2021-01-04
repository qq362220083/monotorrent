//
// IgnoringPicker.cs
//
// Authors:
//   Alan McGovern alan.mcgovern@gmail.com
//
// Copyright (C) 2008 Alan McGovern
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//


using System.Collections.Generic;

namespace MonoTorrent.Client.PiecePicking
{
    public class IgnoringPicker : IPiecePicker
    {
        readonly BitField Bitfield;
        readonly BitField Temp;
        readonly IPiecePicker NextPicker;

        public IgnoringPicker (BitField bitfield, IPiecePicker picker)
        {
            Bitfield = bitfield;
            NextPicker = picker;
            Temp = new BitField (bitfield.Length);
        }

        public int AbortRequests (IPieceRequester peer)
            => NextPicker.AbortRequests (peer);

        public IList<PieceRequest> CancelRequests (IPieceRequester peer, int startIndex, int endIndex)
            => NextPicker.CancelRequests (peer, startIndex, endIndex);

        public PieceRequest ContinueAnyExisting (IPieceRequester peer, int startIndex, int endIndex)
            => NextPicker.ContinueAnyExisting (peer, startIndex, endIndex);

        public PieceRequest ContinueExistingRequest (IPieceRequester peer, int startIndex, int endIndex)
            => NextPicker.ContinueExistingRequest(peer, startIndex, endIndex);

        public int CurrentReceivedCount ()
            => NextPicker.CurrentReceivedCount ();

        public int CurrentRequestCount ()
            => NextPicker.CurrentRequestCount ();

        public IList<PieceRequest> ExportActiveRequests ()
            => NextPicker.ExportActiveRequests ();

        public void Initialise (BitField bitfield, ITorrentData torrentData, IEnumerable<PieceRequest> requests)
            => NextPicker.Initialise (bitfield, torrentData, requests);

        public bool IsInteresting (BitField bitfield)
        {
            Temp.From (bitfield).NAnd (Bitfield);
            return !Temp.AllFalse && NextPicker.IsInteresting (Temp);
        }

        public IList<PieceRequest> PickPiece (IPieceRequester peer, BitField available, IReadOnlyList<IPieceRequester> otherPeers, int count, int startIndex, int endIndex)
        {
            // Invert 'bitfield' and AND it with the peers bitfield
            // Any pieces which are 'true' in the bitfield will not be downloaded
            if (Bitfield.AllFalse)
                return NextPicker.PickPiece (peer, available, otherPeers, count, startIndex, endIndex);

            Temp.From (available).NAnd (Bitfield);
            if (Temp.AllFalse)
                return null;

            return NextPicker.PickPiece (peer, Temp, otherPeers, count, startIndex, endIndex);
        }

        public void RequestRejected (PieceRequest rejectedRequest)
            => NextPicker.RequestRejected (rejectedRequest);

        public void Tick ()
            => NextPicker.Tick ();

        public bool ValidatePiece (IPieceRequester peer, int pieceIndex, int startOffset, int length, out bool pieceComplete, out IList<IPieceRequester> peersInvolved)
            => NextPicker.ValidatePiece (peer, pieceIndex, startOffset, length, out pieceComplete, out peersInvolved);
    }
}
