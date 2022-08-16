﻿using System.Collections.Generic;

namespace _99x8Edit
{
    // Mementos for undo/redo actions
    internal class Memento
    {
        // [For editor] When other data sources are needed, add here
        internal Machine vdpData;
    }
    internal class MementoCaretaker
    {
        private static MementoCaretaker _singleInstance = new MementoCaretaker();
        private List<Memento> mementoList = new List<Memento>();
        private List<Memento> mementoRedo = new List<Memento>();
        private MainWindow UI = null;
        private Machine vdpData;
        internal static MementoCaretaker Instance
        {
            get
            {
                return _singleInstance;
            }
        }
        internal void Initialize(MainWindow ui, Machine vdp)
        {
            UI = ui;
            // [For editor] When other data sources are needed, add here
            vdpData = vdp;
        }
        internal void Push()
        {
            if (vdpData == null)
            {
                // Not initialized
                return;
            }
            Memento m = new Memento();
            {
                // [For editor] When other data sources are needed, add here
                m.vdpData = vdpData.CreateCopy();
            }
            mementoList.Add(m);
            if (mementoList.Count > 256)
            {
                mementoList.RemoveAt(0);
            }
            mementoRedo.Clear();
            UI.UndoEnable = true;
            UI.RedoEnable = false;
        }
        internal void Undo()
        {
            if (mementoList.Count == 0)
            {
                return;
            }
            // Set current status for future redo action
            mementoRedo.Add(this.CreateCurrentMemento());
            // Pop one status from memento list
            Memento m = mementoList[mementoList.Count - 1];
            mementoList.RemoveAt(mementoList.Count - 1);
            {
                // [For editor] When other data sources are needed, add here
                vdpData.SetAllData(m.vdpData);
            }
            if (mementoList.Count == 0)
            {
                UI.UndoEnable = false;
            }
            UI.RedoEnable = true;
        }
        internal void Redo()
        {
            if (mementoRedo.Count == 0)
            {
                return;
            }
            // Push current status to undo list
            mementoList.Add(this.CreateCurrentMemento());
            if (mementoList.Count > 32)
            {
                mementoList.RemoveAt(0);
            }
            // Pop one status from redo list
            Memento m = mementoRedo[mementoRedo.Count - 1];
            mementoRedo.RemoveAt(mementoRedo.Count - 1);
            {
                // [For editor] When other data sources are needed, add here
                vdpData.SetAllData(m.vdpData);
            }
            if (mementoRedo.Count == 0)
            {
                UI.RedoEnable = false;
            }
            UI.UndoEnable = true;
        }
        internal void Clear()
        {
            mementoList.Clear();
            mementoRedo.Clear();
            UI.UndoEnable = false;
            UI.RedoEnable = false;
        }
        // Utility
        private Memento CreateCurrentMemento()
        {
            Memento current = new Memento();
            {
                // [For editor] When other data sources are needed, add here
                current.vdpData = vdpData.CreateCopy();
            }
            return current;
        }
    }
}
