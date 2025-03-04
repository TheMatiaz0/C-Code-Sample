using System.Collections.Generic;
using Cyberultimate.Unity;
using Telegraphist.Gameplay.HandPaperVariant.Tiles;
using Telegraphist.Gameplay.Tiles;
using Telegraphist.Gameplay.TileSpawner;
using Telegraphist.Helpers;
using Telegraphist.Scriptables;
using Telegraphist.TileSystem;
using UnityEngine;

namespace Telegraphist.Gameplay.HandPaperVariant
{
    public record TileParams(Vector2 Position, Vector2 Scale, int PageIndex, bool IsNewLine, bool IsNewPage);
    
    public class TileSpawnerPaper : TileSpawnerBase
    {
        [Header("References")]
        [SerializeField] private HandPaperHelpers helpers;
        [SerializeField] private PaperMovementController controller;
        [SerializeField] private PaperPhantomTiles phantomTiles;
        [SerializeField] private Transform pagesRoot;

        [Header("Prefabs")]
        [SerializeField] private PageObject pagePrefab;
        [SerializeField] private GameObject basePage;
        [SerializeField] private GameObject pageLineDivider;
        [SerializeField] private GameObject lineBreakPrefab;
        
        [Header("Config")]
        [SerializeField] private float lineDividersOffsetY;
        [SerializeField] private bool spawnLineBreakIcons = true;
        
        private List<PageObject> pages;

        protected override bool ShouldDestroyTilesOutsideScreen => false;
        private LevelScriptable Level => LevelContext.Current.Level;

        protected override void Start()
        {
            base.Start();
            basePage.SetActive(false);
        }

        protected override void BeforeTilesSpawn()
        {
            base.BeforeTilesSpawn();
            helpers.Setup();
            SetupPages();
            controller.Setup(pages);
            phantomTiles.Setup();
        }

        protected override void OnSongUpdate(float time)
        {
            base.OnSongUpdate(time);
            controller.MoveHandAndPage(time);
        }

        protected override TileObjectBase SpawnTile(Tile tile, int index, int frequencyContainerIndex)
        {
            var (position, scale, pageIndex, isNewLine, isNewPage) = GetTileParams(tile, index);
            
            if (pageIndex >= pages.Count)
            {
                Debug.LogError($"Tile #{index} is out of song bounds! Fix this.");
                return null;
            }
            
            var parent = pages[pageIndex].FrequencyContainers[frequencyContainerIndex];
            
            var go = Instantiate(tilePrefab, parent);
            var tileScript = go.GetComponent<PaperTile>();
            tileScript.Init(tile, helpers.OffsetPosition(position), scale);

            var phantomPageIndex = isNewPage ? pageIndex - 1 : pageIndex;
            var phantomExists = phantomTiles.OnePerRow && phantomTiles.Contains(GetPhantomTileKey(tile, pageIndex));
            if ((isNewLine || isNewPage) && !phantomExists)
            {
                SpawnPhantomTile(tile, index, phantomPageIndex, initiallyVisible: false);
                var tileBehaviour = TileController.Current.GetTileHandle<MorseTileBehaviour>(index);
                tileBehaviour.HasMoreTolerance = true;
            }

            return tileScript;
        }
        
        private TileParams GetTileParams(Tile tile, int index)
        {
            var pageIndex = helpers.GetPageForBeat(tile.StartBeat);
            
            var pageStartBeat = helpers.RelativePageBeat(tile.StartBeat, pageIndex);
            var pageEndBeat = helpers.RelativePageBeat(tile.EndBeat, pageIndex);
            var position = helpers.BeatToPosition(pageStartBeat, pageEndBeat);
            
            var width = helpers.BeatDurationToWidth(tile.Duration);
            var scale = new Vector2(width, helpers.RowHeight);
            
            var isNewPage = pageStartBeat < phantomTiles.NewPageThreshold && pageIndex > 0;
            var isNewLine = position.x < phantomTiles.NewLineThreshold && pageIndex > 0;
            
            return new TileParams(position, scale, pageIndex, isNewLine, isNewPage);
        }

        private (int page, int row) GetPhantomTileKey(Tile tile, int pageIndex)
        {
            var startBeat = helpers.RelativePageBeat(tile.StartBeat, pageIndex);
            var rowIndex = helpers.GetBeatRow(startBeat) - 1;
            return (pageIndex, rowIndex);
        }

        private void SpawnPhantomTile(Tile tile, int tileIndex, int pageIndex, bool initiallyVisible)
        {
            var startBeat = helpers.RelativePageBeat(tile.StartBeat, pageIndex);
            var endBeat = helpers.RelativePageBeat(tile.EndBeat, pageIndex);
            var position = helpers.BeatToPosition(startBeat, endBeat);
            
            // move to previous line
            position.y += helpers.ActualRowHeight;
            position.x += helpers.PageSize.x;
            
            var width = helpers.BeatDurationToWidth(tile.Duration);
            var scale = new Vector3(width, helpers.RowHeight);
            
            var go = Instantiate(tilePrefab, pages[pageIndex].transform);
            var tileScript = go.GetComponent<PaperTile>();
            tileScript.Init(tile, helpers.OffsetPosition(position), scale);
            tileScript.SetPhantom(initiallyVisible);
            
            var rowIndex = helpers.GetBeatRow(startBeat) - 1;
            phantomTiles.Add(pageIndex, rowIndex, tileScript);
        }

        private void SetupPages()
        {
            pagesRoot.KillAllChildren();

            var pageCount = helpers.PageCount;
            pages = new List<PageObject>(pageCount);
            for (var i = 0; i <= pageCount; i++)
            {
                var page = Instantiate(pagePrefab, pagesRoot);
                page.Setup(FrequencyContainers, Level.IsRanked);
                page.gameObject.SetActive(false);
                pages.Add(page);
                SpawnDecorations(page.transform);
            }
            
            pages[0].gameObject.SetActive(true);
        }
        
        private void SpawnDecorations(Transform root)
        {
            for (var i = 0; i < helpers.RowsInPage; i++)
            {
                SpawnLineDivider(root, i);
                SpawnLineBreakIcon(root, i);
            }
        }

        private void SpawnLineDivider(Transform root, int lineIndex)
        {
            var go = Instantiate(pageLineDivider, root);
            var y = (lineIndex + 1) * helpers.ActualRowHeight * -1;
            go.transform.localPosition = new Vector3(0, helpers.PageExtents.y + y + lineDividersOffsetY);
        }

        private void SpawnLineBreakIcon(Transform root, int lineIndex)
        {
            if (!spawnLineBreakIcons) return;
            
            var go = Instantiate(lineBreakPrefab, root);
            var spriteRenderer = go.GetComponentInChildren<SpriteRenderer>();
            var y = lineIndex * helpers.ActualRowHeight * -1 - helpers.ActualRowHeight / 2;
            var x = helpers.PageExtents.x - helpers.BeatDurationToWidth(controller.HandEarlyOffsetBeats) + spriteRenderer.bounds.size.x;
            go.transform.localPosition = new Vector3(x, helpers.PageExtents.y + y);
        }
    }
}