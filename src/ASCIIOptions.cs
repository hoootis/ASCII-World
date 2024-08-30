namespace ASCIIWorld;

public class ASCIIOptions : OptionInterface
{
    public static readonly ASCIIOptions Instance = new();
    
    public static Configurable<float> contrast;
    public static float contrastF;
    public static Configurable<float> offset;
    public static float offsetF;
    public static Configurable<bool> asciiBool;
    public static bool asciiBoolF;
    public static Configurable<string> ASCIITex;
    public static string ASCIITexF;
    
    public static List<ListItem> asciiTextures = new();

    [CanBeNull] public static UIelement[] uIelements;

    public ASCIIOptions()
    {
        contrast = config.Bind<float>("ASCIIWorld_contrast", 150, new ConfigAcceptableRange<float>(0, 300));
        offset = config.Bind<float>("ASCIIWorld_offset", 20, new ConfigAcceptableRange<float>(0, 100));
        ASCIITex = config.Bind<string>("ASCIIWorld_texture", "KarmaASCII");
        asciiBool = config.Bind<bool>("ASCIIWorld_enable", false);
    }

    public override void Initialize()
    {
        OpTab opTab = new(this, "Options");
        Tabs = new[]
        {
            opTab
        };
        
        const int sliderBarLength = 135;
        const int rightSidePos = 360;
        const int leftSidePos = 60;
        #nullable enable

        uIelements = new UIelement[]
        {
            new OpLabel(200, 575, Translate("ASCII Shader Options"), true) {alignment=FLabelAlignment.Center},
            
            // Make the options on the left side
            new OpCheckBox(asciiBool, new Vector2(leftSidePos, 520)) {description=Translate("Toggles the ASCII Shader")},
            new OpLabel(leftSidePos+30, 523, Translate("ASCII Toggle")),

            new OpFloatSlider(contrast, new Vector2(leftSidePos, 440), sliderBarLength) {description=Translate("Brightness of gameboy lines.")},
            new OpLabel(leftSidePos, 415, Translate("\nBrightness of gameboy grid. \nValues lower than 50 are darker, \nhigher than 50 are brighter.")),
            
            new OpFloatSlider(offset, new Vector2(leftSidePos, 200), sliderBarLength) {description=Translate("Applies the VCR Shader effect")},
            new OpLabel(leftSidePos+30, 203, Translate("VCR Effect")),
            
            new OpComboBox(ASCIITex, new Vector2(leftSidePos, 140), 100, asciiTextures),
            new OpLabel(leftSidePos, 120, Translate("ASCII Text Choice")),
        };
        opTab.AddItems(uIelements);
    }

    public override void Update()
    {
        if (uIelements != null)
        {
            asciiBoolF = ((OpCheckBox)uIelements[1]).GetValueBool();
            contrastF = ((OpFloatSlider)uIelements[3]).GetValueFloat();
            offsetF = ((OpFloatSlider)uIelements[5]).GetValueFloat();
            ASCIITexF = ((OpComboBox)uIelements[7])._GetDisplayValue();
        }
    }
}