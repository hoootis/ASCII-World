global using System.Collections.Generic;
global using BepInEx.Logging;
global using JetBrains.Annotations;
global using Menu.Remix.MixedUI;
global using Menu.Remix.MixedUI.ValueTypes;
global using UnityEngine;
global using System;
global using System.Linq;
global using System.Text;
global using System.Threading.Tasks;
global using BepInEx;
global using System.Security.Permissions;
global using RWCustom;
global using System.Security;
global using System.Reflection;
global using MonoMod.RuntimeDetour;
global using UnityEngine.Assertions;
global using UnityEngine.PlayerLoop;
global using System.IO;
using On.Menu;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace ASCIIWorld;

[BepInPlugin(GUID: MOD_ID, Name: MOD_NAME, Version: VERSION)]
sealed class Plugin : BaseUnityPlugin
{
    public const string MOD_ID = "cactus.ascii";
    public const string MOD_NAME = "ASCII";
    public const string VERSION = "1.0";
    public const string AUTHORS = "ASlightlyOverGrownCactus";
    
    static bool loaded = false;
    public static Shader ASCIIShader;
    public static Shader ASCIIStencil;
    public static Texture2D[] asciiTextures = new Texture2D[2];
    public static MaterialPropertyBlock asciiTexBlock;

    public void OnEnable()
    {
        try
        {
            On.RainWorld.OnModsInit += RainWorldOnOnModsInit;
            On.Menu.Dialog.ctor_ProcessManager += DialogOnctor_ProcessManager;
            On.FLabel.ctor_string_string += FLabelOnctor_string_string;
            On.FButton.ctor_string_string_string_string += FButtonOnctor_string_string_string_string;
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            Debug.LogException(e);
            throw new Exception("Exception from ASCIIWorld: " + e);
        }
    }

    private void FButtonOnctor_string_string_string_string(On.FButton.orig_ctor_string_string_string_string orig, FButton self, string upelementname, string downelementname, string overelementname, string clicksoundname)
    {
        orig(self, upelementname, downelementname, overelementname, clicksoundname);
        self.sprite.shader = FShader.CreateShader("ASCIIStencil", ASCIIStencil);
    }

    private void FLabelOnctor_string_string(On.FLabel.orig_ctor_string_string orig, FLabel self, string fontname, string text)
    {
        orig(self, fontname, text);
        self.shader = FShader.CreateShader("ASCIIStencil", ASCIIStencil);
    }

    private void DialogOnctor_ProcessManager(Dialog.orig_ctor_ProcessManager orig, Menu.Dialog self, ProcessManager manager)
    {
        orig(self, manager);
        self.darkSprite.shader = FShader.CreateShader("ASCIIStencil", ASCIIStencil);
    }

    private void RainWorldOnOnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);
        try
        {
            if (loaded) return;
            loaded = true;
            ShaderBuffer.Initialize();

            MachineConnector.SetRegisteredOI("cactus.ascii", ASCIIOptions.Instance);
            var bundle = AssetBundle.LoadFromFile(AssetManager.ResolveFilePath("assets/asciiworld")); // Load asset bundle from assets folder

            asciiTexBlock = new();
            
            // Ascii textures loading
            asciiTextures[0] = bundle.LoadAsset<Texture2D>("Assets/ASCIIPROJECT/Textures/newASCII2.png");
            ASCIIOptions.asciiTextures.Add(new ListItem("KarmaASCII"));
            asciiTextures[1] = bundle.LoadAsset<Texture2D>("Assets/ASCIIPROJECT/Textures/1x0 8x8 3.png");
            ASCIIOptions.asciiTextures.Add(new ListItem("AcerolaASCII"));
            
            // Shaders
            ASCIIShader = bundle.LoadAsset<Shader>("Assets/ASCIIPROJECT/ASCII.shader"); // Loads shader from asset bundle.
            ASCIIStencil = bundle.LoadAsset<Shader>("Assets/ASCIIPROJECT/BasicStencil.shader");
            Camera.main.gameObject.AddComponent<ASCIIScreen>();
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            Debug.LogException(e);
            throw new Exception("Error trying to load OnModsInit ASCIIWorld");
        }
    }
}

public class ASCIIScreen : MonoBehaviour
{
    private static bool menuLoaded = false;

    public Material asciiMat;
    [Range(0, 3)] public float contrast = 1.3f;
    [Range(0, 1)] public float offset = 0.15f;
    private bool useShader = false;
    
    private void Awake()
    {
        asciiMat = new Material(Plugin.ASCIIShader);
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        // TODO: Add in contrast and offset setting
        asciiMat.SetTexture("_ASCIIKarmaTex", Plugin.asciiTexBlock.GetTexture("_ASCIIKarmaTex"));
        Graphics.Blit(src, dest, asciiMat);
    }

    private void Update()
    {
        if (menuLoaded)
        {
            Plugin.asciiTexBlock.SetTexture("_ASCIIKarmaTex", GetASCII(ASCIIOptions.ASCIITexF));
            Plugin.asciiTexBlock.SetFloat("_TexWidth", GetASCIIWidth(ASCIIOptions.ASCIITexF));

            contrast = ASCIIOptions.contrastF / 100f;
            offset = ASCIIOptions.offsetF / 100f;
            useShader = ASCIIOptions.asciiBoolF;
        }
        else
        {
            Plugin.asciiTexBlock.SetTexture("_ASCIIKarmaTex", GetASCII(ASCIIOptions.ASCIITex.Value));
            Plugin.asciiTexBlock.SetFloat("_TexWidth", GetASCIIWidth(ASCIIOptions.ASCIITex.Value));

            contrast = ASCIIOptions.contrast.Value / 100f;
            offset = ASCIIOptions.offset.Value / 100f;
            useShader = ASCIIOptions.asciiBool.Value;
        }
    }
    
    public Texture2D GetASCII(string ascii)
    {
        Texture2D asciiTex = ascii switch
        {
            "KarmaASCII" => Plugin.asciiTextures[0],
            _ => Plugin.asciiTextures[1],
        };
        return asciiTex;
    }

    public float GetASCIIWidth(string ascii)
    {
        float asciiWidth = ascii switch
        {
            "KarmaASCII" => 16,
            _ => 8,
        };
        return asciiWidth;
    }
}
