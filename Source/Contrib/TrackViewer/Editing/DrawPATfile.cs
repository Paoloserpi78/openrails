﻿// COPYRIGHT 2014 by the Open Rails project.
// 
// This file is part of Open Rails.
// 
// Open Rails is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// Open Rails is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Open Rails.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

using MSTS.Formats;
using ORTS.Common;
using ORTS.TrackViewer.Drawing;

namespace ORTS.TrackViewer.Editing
{
    /// <summary>
    /// Class to draw a raw path from PATfile. With raw here is meant that there is no link whatsoever to the trackdatabase
    /// So only locations as defined in .pat file are used. Obviously for broken paths there can be quite a mismatch between 
    /// locations in the .pat file and those of the tracks.
    /// The drawing here is not updated during editing.
    /// 
    /// Main method is Draw.
    /// 
    /// The amount of points that are drawn can be varied, such that it is easier to follow the path (especially in 
    /// complicated cases.
    /// </summary>
    public class DrawPATfile
    {

        private PATFile patFile;
        public string fileName;

        private int numberToDraw = 4;   // Number of nodes that will be drawn. Start with a few
        private int currentMainNodeIndex;       // Number of the currentIndex

        public TrPathNode CurrentNode { get { return patFile.TrPathNodes[currentMainNodeIndex]; } }
        public TrackPDP CurrentPDP { get { return patFile.TrackPDPs[(int)CurrentNode.fromPDP]; } }
        public WorldLocation CurrentLocation { get { return getPDPLocation(CurrentPDP); } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="path">Contains the information (mainly filepath) needed for loading the .pat file</param>
        public DrawPATfile (ORTS.Menu.Path path)
        {
            fileName = path.FilePath.Split('\\').Last();
            patFile = new PATFile(path.FilePath);
        }

        /// <summary>
        /// Draw the actual path coded in the PATfile (for a number of nodes that can be extended or reduced)
        /// </summary>
        /// <param name="drawArea">Area to draw upon</param>
        public void Draw(DrawArea drawArea)
        {
            
            //draw actual path
            currentMainNodeIndex = 0; // starting point
            int currentSidingNodeIndex = -1; // we start without siding path
            for (int i = 0; i < Math.Min(patFile.TrPathNodes.Count - 1, numberToDraw); i++)
            {

                // If we have a current siding track, we draw it step to the next main line first.
                if (currentSidingNodeIndex > 0)
                {
                    //while tracking a siding, it has its own main node
                    int nextNodeIndexOnSiding = (int)patFile.TrPathNodes[currentSidingNodeIndex].nextSidingNode;
                    if (nextNodeIndexOnSiding > 0) // because also this path can run off at the end
                    {
                        TrPathNode curNode = patFile.TrPathNodes[currentSidingNodeIndex];
                        WorldLocation curLoc = getPDPLocation(patFile.TrackPDPs[(int)curNode.fromPDP]);
                        TrPathNode nextNode = patFile.TrPathNodes[nextNodeIndexOnSiding];
                        WorldLocation nextLoc = getPDPLocation(patFile.TrackPDPs[(int)nextNode.fromPDP]);

                        drawArea.DrawLine(1, DrawColors.colorsNormal["pathSiding"], curLoc, nextLoc);
                    }
                    currentSidingNodeIndex = nextNodeIndexOnSiding;
                }

                TrPathNode curMainNode = patFile.TrPathNodes[currentMainNodeIndex];
                WorldLocation curMainLoc = getPDPLocation(patFile.TrackPDPs[(int)curMainNode.fromPDP]);
                
                // from this main line point to the next siding node.
                // If there is a next siding node, we also reset the currentSidingNodeIndex
                // but probably it is not allowed to have siding
                int nextSidingNodeIndex = (int)curMainNode.nextSidingNode;             
                if (nextSidingNodeIndex >= 0)
                {
                    // draw the start of a siding path
                    TrPathNode nextNode = patFile.TrPathNodes[nextSidingNodeIndex];
                    WorldLocation nextLoc = getPDPLocation(patFile.TrackPDPs[(int)nextNode.fromPDP]);

                    drawArea.DrawLine(1, DrawColors.colorsNormal["pathSiding"], curMainLoc, nextLoc);
                    currentSidingNodeIndex = nextSidingNodeIndex;
                }

                // From this main line point to the next
                int nextMainNodeIndex = (int)curMainNode.nextMainNode; 
                if (nextMainNodeIndex >= 0)
                {
                    TrPathNode nextNode = patFile.TrPathNodes[nextMainNodeIndex];
                    WorldLocation nextLoc = getPDPLocation(patFile.TrackPDPs[(int)nextNode.fromPDP]);

                    drawArea.DrawLine(1, DrawColors.colorsNormal["pathMain"], curMainLoc, nextLoc);
                    currentMainNodeIndex = nextMainNodeIndex;
                }
            }
 
        }

        /// <summary>
        /// Convert a PDP with raw coordinates numbers to a world location
        /// </summary>
        /// <param name="pdp">The trackPDP</param>
        /// <returns>The corresponding world location</returns>
        private static WorldLocation getPDPLocation(TrackPDP pdp)
        {
            return new WorldLocation(pdp.TileX, pdp.TileZ, pdp.X, pdp.Y, pdp.Z);
        }

        /// <summary>
        /// Draw more sections of the path
        /// </summary>
        public void extendPath()
        {
            int maxNumber = patFile.TrPathNodes.Count-1;
            if (++numberToDraw > maxNumber) numberToDraw = maxNumber;
        }

        /// <summary>
        /// Draw the full (complete) path
        /// </summary>
        public void extendPathFull()
        {
            numberToDraw = patFile.TrPathNodes.Count - 1;
        }

        /// <summary>
        /// Draw less sections of the path
        /// </summary>
        public void reducePath()
        {
            if (--numberToDraw < 0) numberToDraw = 0;
        }

        /// <summary>
        /// Go to initial node and draw no path sections
        /// </summary>
        public void reducePathFull()
        {
            numberToDraw = 0;
        }
    }
}
