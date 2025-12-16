// World Political Map - Globe Edition for Unity - Main Script
// Created by Ramiro Oliva (Kronnect)
// Don't modify this script - changes could be lost if you upgrade to a more recent version of WPM

using UnityEngine;
using System.Collections.Generic;
using WPM.ClipperLib;

namespace WPM {

    public partial class WorldMapGlobe : MonoBehaviour {


		/// <summary>
		/// Merges adjacent regions of an entity
		/// </summary>
		/// <param name="entity"></param>
		public void MergeAdjacentRegions(IAdminEntity entity) {
			// Searches for adjacency - merges in first region
			List<Region> regionsToAdd = new List<Region>();
			int regionCount = entity.regions.Count;
			for (int k = 0; k < regionCount; k++) {
				Region region1 = entity.regions[k];
				if (region1 == null || region1.latlon == null || region1.latlon.Length == 0)
					continue;
				regionsToAdd.Clear();
				for (int j = k + 1; j < regionCount; j++) {
					Region region2 = entity.regions[j];
					if (region2 == null || region2.latlon == null || region2.latlon.Length == 0)
						continue;
					if (!region1.Intersects(region2))
						continue;

					regionsToAdd.Add(region2);

					// Add new neighbours
					int rnCount = region2.neighbours.Count;
					for (int n = 0; n < rnCount; n++) {
						Region neighbour = region2.neighbours[n];
						if (neighbour != null && neighbour != region1 && !region1.neighbours.Contains(neighbour)) {
							region1.neighbours.Add(neighbour);
						}
					}
					// Remove merged region
					entity.regions.RemoveAt(j);
					j = k;
					regionCount--;
				}
				if (regionsToAdd.Count > 0) {
					region1.sanitized = false;
					entity.mainRegionIndex = 0; // will need to refresh country definition later in the process
					Clipper clipper = new Clipper();
					foreach (Region region in regionsToAdd) {
						RegionMagnet(region1, region);
						clipper.AddPath(region, PolyType.ptClip);
					}
					clipper.AddPath(region1, PolyType.ptSubject);
					clipper.Execute(ClipType.ctUnion, region1);
				}
			}
		}
	}

}