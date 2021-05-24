﻿using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UI;
using UnityStandardAssets.ImageEffects;

namespace AugaUnity
{
    public enum PortraitMode
    {
        Hair,
        Beard
    }

    public class CharacterPortraitsController : MonoBehaviour
    {
        public CharacterPortrait PortraitPrefab;
        public RectTransform PortraitList;
        public RenderTexture RenderTexture;
        public PortraitMode Mode;

        private Camera _camera;
        private Transform _lookTarget;
        private float _fov = 11;
        private Vector3 _offset = new Vector3(0, -0.05f, 0);
        private PlayerCustomizaton _playerCustomizaton;
        private readonly List<CharacterPortrait> _characterPortraits = new List<CharacterPortrait>();

        [UsedImplicitly]
        public void Awake()
        {
            _playerCustomizaton = FejdStartup.instance.m_newCharacterPanel.GetComponent<PlayerCustomizaton>();

            _camera = Instantiate(FejdStartup.instance.m_mainCamera.GetComponent<Camera>());
            _camera.fieldOfView = _fov;
            _camera.targetTexture = RenderTexture;
            _camera.GetComponent<DepthOfField>().enabled = false;
            _camera.enabled = false;

            _camera.transform.position = FejdStartup.instance.m_cameraMarkerCharacter.position;
            _camera.transform.rotation = FejdStartup.instance.m_cameraMarkerCharacter.rotation;
        }

        [UsedImplicitly]
        public void Start()
        {
            _lookTarget = Utils.FindChild(FejdStartup.instance.m_playerInstance.transform, "Head");

            var count = Mode == PortraitMode.Hair ? _playerCustomizaton.m_hairs.Count : _playerCustomizaton.m_beards.Count;
            for (var i = 0; i < count; i++)
            {
                var characterPortrait = Instantiate(PortraitPrefab, PortraitList);
                var index = i;
                characterPortrait.Button.onClick.AddListener(() => OnClick(index));
                characterPortrait.Setup(_playerCustomizaton, Mode, index);
                _characterPortraits.Add(characterPortrait);
            }
        }

        public void OnClick(int index)
        {
            switch (Mode)
            {
                case PortraitMode.Hair:
                    _playerCustomizaton.SetHair(index);
                    break;

                default:
                    _playerCustomizaton.SetBeard(index);
                    break;
            }
        }

        public void Update()
        {
            _camera.transform.LookAt(_lookTarget.position + _offset);

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                _offset += Vector3.down * 0.05f;
                PrintOffset();
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                _offset += Vector3.up * 0.05f;
                PrintOffset();
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                _offset += Vector3.forward * 0.05f;
                PrintOffset();
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                _offset += Vector3.back * 0.05f;
                PrintOffset();
            }

            if (Input.GetKeyDown(KeyCode.KeypadPlus))
            {
                _camera.fieldOfView -= 0.5f;
                PrintFOV();
            }
            else if (Input.GetKeyDown(KeyCode.KeypadMinus))
            {
                _camera.fieldOfView += 0.5f;
                PrintFOV();
            }
        }

        public void PrintOffset()
        {
            Debug.Log($"Offset: {_offset}");
        }

        public void PrintFOV()
        {
            Debug.Log($"FOV: {_camera.fieldOfView:0.0}");
        }

        [UsedImplicitly]
        public void LateUpdate()
        {
            var player = _playerCustomizaton.GetPlayer();
            var visEquip = player.m_visEquipment;
            var itemInstance = Mode == PortraitMode.Hair ? visEquip.m_hairItemInstance : visEquip.m_beardItemInstance;
            itemInstance?.SetActive(false);

            foreach (var characterPortrait in _characterPortraits)
            {
                characterPortrait.DoRender(visEquip, _camera);
            }

            itemInstance?.SetActive(true);
        }
    }

    public class CharacterPortrait : MonoBehaviour
    {
        public RawImage Image;
        public Button Button;

        private Texture _texture;
        private GameObject _attachedItem;
        private List<Renderer> _renderers;

        public void Setup(PlayerCustomizaton playerCustomizaton, PortraitMode mode, int index)
        {
            var player = playerCustomizaton.GetPlayer();
            var visEquip = player.m_visEquipment;
            var items = mode == PortraitMode.Hair ? playerCustomizaton.m_hairs : playerCustomizaton.m_beards;
            var itemName = items[index].gameObject.name;
            var itemHash = itemName.GetStableHashCode();

            _attachedItem = visEquip.AttachItem(itemHash, 0, visEquip.m_helmet);
            _renderers = _attachedItem != null ? _attachedItem.GetComponentsInChildren<Renderer>().ToList() : new List<Renderer>();
        }

        public void DoRender(VisEquipment visEquip, Camera camera)
        {
            var hairColor = Utils.Vec3ToColor(visEquip.m_nview.GetZDO()?.GetVec3("HairColor", Vector3.one) ?? visEquip.m_hairColor);
            foreach (var renderer in _renderers)
            {
                renderer.forceRenderingOff = false;
                renderer.material.SetColor("_SkinColor", hairColor);
            }

            camera.Render();
            SetTexture(camera.targetTexture);

            foreach (var renderer in _renderers)
            {
                renderer.forceRenderingOff = true;
            }
        }

        public void SetTexture(RenderTexture renderTexture)
        {
            if (_texture == null)
            {
                _texture = new Texture2D(renderTexture.width, renderTexture.height, renderTexture.graphicsFormat, renderTexture.mipmapCount, TextureCreationFlags.None);
                Image.texture = _texture;
            }

            Graphics.CopyTexture(renderTexture, _texture);
        }

        [UsedImplicitly]
        public void OnDestroy()
        {
            _renderers.Clear();
            Destroy(_attachedItem);
            Destroy(_texture);
        }
    }
}