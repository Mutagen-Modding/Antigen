using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Placed.Object;

public class LocRefTypeAnalyzer : IContextualRecordAnalyzer<IPlacedGetter>
{
    public static readonly TopicDefinition<IFormLinkGetter> InvalidHouseBedRefType = MutagenTopicBuilder.FromDiscussion(
            408,
            "Invalid HouseBedRefType",
            Severity.Warning)
        .WithFormatting<IFormLinkGetter>("Placed has Location Reference Type HouseBedRefType but is not a bed, but {0}");

    public static readonly TopicDefinition<IFormLinkGetter> InvalidHouseContainerRefType = MutagenTopicBuilder.FromDiscussion(
            409,
            "Invalid HouseContainerRefType",
            Severity.Warning)
        .WithFormatting<IFormLinkGetter>("Placed has Location Reference Type HouseContainerRefType but is not a container, but {0}");

    public static readonly TopicDefinition<IFormLinkGetter> InvalidBossContainer = MutagenTopicBuilder.FromDiscussion(
            410,
            "Invalid BossContainer",
            Severity.Warning)
        .WithFormatting<IFormLinkGetter>("Placed has Location Reference Type BossContainer but is not a container, but {0}");

    public static readonly TopicDefinition<IFormLinkGetter> InvalidMerchantContainerRefType = MutagenTopicBuilder.FromDiscussion(
            411,
            "Invalid MerchantContainerRefType",
            Severity.Warning)
        .WithFormatting<IFormLinkGetter>("Placed has Location Reference Type MerchantContainerRefType but is not a door, but {0}");

    public static readonly TopicDefinition<IFormLinkGetter> InvalidHouseMainDoorRefType = MutagenTopicBuilder.FromDiscussion(
            412,
            "Invalid HouseMainDoorRefType",
            Severity.Warning)
        .WithFormatting<IFormLinkGetter>("Placed has Location Reference Type HouseMainDoorRefType but is not a door, but {0}");

    public static readonly TopicDefinition<IFormLinkGetter> InvalidHouseBackDoorRefType = MutagenTopicBuilder.FromDiscussion(
            413,
            "Invalid HouseBackDoorRefType",
            Severity.Warning)
        .WithFormatting<IFormLinkGetter>("Placed has Location Reference Type HouseBackDoorRefType but is not an door, but {0}");

    public static readonly TopicDefinition<IFormLinkGetter> InvalidWETravel = MutagenTopicBuilder.FromDiscussion(
            414,
            "Invalid WETravel",
            Severity.Warning)
        .WithFormatting<IFormLinkGetter>("Placed has Location Reference Type LocationCenterMarker but is not an xMarker, but {0}");

    public static readonly TopicDefinition<IFormLinkGetter> InvalidWEScene = MutagenTopicBuilder.FromDiscussion(
            415,
            "Invalid WEScene",
            Severity.Warning)
        .WithFormatting<IFormLinkGetter>("Placed has Location Reference Type WEScene but is not an xMarkerHeading, but {0}");

    public static readonly TopicDefinition<IFormLinkGetter> InvalidWESceneCenter = MutagenTopicBuilder.FromDiscussion(
            416,
            "Invalid WESceneCenter",
            Severity.Warning)
        .WithFormatting<IFormLinkGetter>("Placed has Location Reference Type WESceneCenter but is not an xMarkerHeading, but {0}");

    public static readonly TopicDefinition<IFormLinkGetter> InvalidMapMarkerRefType = MutagenTopicBuilder.FromDiscussion(
            417,
            "Invalid MapMarkerRefType",
            Severity.Warning)
        .WithFormatting<IFormLinkGetter>("Placed has Location Reference Type MapMarkerRefType but is not a MapMarker, but {0}");

    public static readonly TopicDefinition<IFormLinkGetter> InvalidBoss = MutagenTopicBuilder.FromDiscussion(
            418,
            "Invalid Boss",
            Severity.Warning)
        .WithFormatting<IFormLinkGetter>("Placed has Location Reference Type Boss but is not an Npc, but {0}");

    public IEnumerable<TopicDefinition> Topics { get; } = [
        InvalidHouseBedRefType,
        InvalidHouseContainerRefType,
        InvalidBossContainer,
        InvalidMerchantContainerRefType,
        InvalidHouseMainDoorRefType,
        InvalidHouseBackDoorRefType,
        InvalidWETravel,
        InvalidWEScene,
        InvalidWESceneCenter,
        InvalidMapMarkerRefType,
        InvalidBoss,
    ];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<IPlacedGetter> param)
    {
        var placed = param.Record;
        if (placed.LocationRefTypes is null) return;

        foreach (var locationRefType in placed.LocationRefTypes)
        {
            if (locationRefType.Equals(FormKeys.SkyrimSE.Skyrim.LocationReferenceType.HouseBedRefType))
            {
                CheckObject(placeableObject => placeableObject is not IFurnitureGetter furniture || !furniture.IsBed(), InvalidHouseBedRefType);
            }
            else if (locationRefType.Equals(FormKeys.SkyrimSE.Skyrim.LocationReferenceType.HouseContainerRefType))
            {
                CheckObject(placeableObject => placeableObject is not IContainerGetter, InvalidHouseContainerRefType);
            }
            else if (locationRefType.Equals(FormKeys.SkyrimSE.Skyrim.LocationReferenceType.BossContainer))
            {
                CheckObject(placeableObject => placeableObject is not IContainerGetter, InvalidBossContainer);
            }
            else if (locationRefType.Equals(FormKeys.SkyrimSE.Skyrim.LocationReferenceType.MerchantContainerRefType))
            {
                CheckObject(placeableObject => placeableObject is not IContainerGetter, InvalidMerchantContainerRefType);
            }
            else if (locationRefType.Equals(FormKeys.SkyrimSE.Skyrim.LocationReferenceType.HouseMainDoorRefType))
            {
                CheckObject(placeableObject => placeableObject is not IDoorGetter, InvalidHouseMainDoorRefType);
            }
            else if (locationRefType.Equals(FormKeys.SkyrimSE.Skyrim.LocationReferenceType.HouseBackDoorRefType))
            {
                CheckObject(placeableObject => placeableObject is not IDoorGetter, InvalidHouseBackDoorRefType);
            }
            else if (locationRefType.Equals(FormKeys.SkyrimSE.Skyrim.LocationReferenceType.WETravel))
            {
                CheckObject(placeableObject => placeableObject.FormKey != FormKeys.SkyrimSE.Skyrim.Static.XMarker.FormKey, InvalidWETravel);
            }
            else if (locationRefType.Equals(FormKeys.SkyrimSE.Skyrim.LocationReferenceType.WEScene))
            {
                CheckObject(placeableObject => placeableObject.FormKey != FormKeys.SkyrimSE.Skyrim.Static.XMarkerHeading.FormKey, InvalidWEScene);
            }
            else if (locationRefType.Equals(FormKeys.SkyrimSE.Skyrim.LocationReferenceType.WESceneCenter))
            {
                CheckObject(placeableObject => placeableObject.FormKey != FormKeys.SkyrimSE.Skyrim.Static.XMarkerHeading.FormKey, InvalidWESceneCenter);
            }
            else if (locationRefType.Equals(FormKeys.SkyrimSE.Skyrim.LocationReferenceType.MapMarkerRefType))
            {
                CheckObject(placeableObject => placeableObject.FormKey != FormKeys.SkyrimSE.Skyrim.Static.MapMarker.FormKey, InvalidMapMarkerRefType);
            }
            else if (locationRefType.Equals(FormKeys.SkyrimSE.Skyrim.LocationReferenceType.Boss))
            {
                CheckNpc(_ => false, InvalidBoss);
            }
        }

        void CheckObject(Func<IPlaceableObjectGetter, bool> isInvalid, TopicDefinition<IFormLinkGetter> topic)
        {
            switch (placed)
            {
                case IPlacedArrowGetter placedGetter:
                {
                    param.AddTopic(
                        topic.Format(placedGetter.Projectile));
                    break;
                }
                case IPlacedBarrierGetter placedGetter:
                {
                    param.AddTopic(
                        topic.Format(placedGetter.Projectile));
                    break;
                }
                case IPlacedBeamGetter placedGetter:
                {
                    param.AddTopic(
                        topic.Format(placedGetter.Projectile));
                    break;
                }
                case IPlacedConeGetter placedGetter:
                {
                    param.AddTopic(
                        topic.Format(placedGetter.Projectile));
                    break;
                }
                case IPlacedFlameGetter placedGetter:
                {
                    param.AddTopic(
                        topic.Format(placedGetter.Projectile));
                    break;
                }
                case IPlacedHazardGetter placedGetter:
                {
                    param.AddTopic(
                        topic.Format(placedGetter.Hazard));
                    break;
                }
                case IPlacedMissileGetter placedGetter:
                {
                    param.AddTopic(
                        topic.Format(placedGetter.Projectile));
                    break;
                }
                case IPlacedTrapGetter placedGetter:
                {
                    param.AddTopic(
                        topic.Format(placedGetter.Projectile));
                    break;
                }
                case IPlacedObjectGetter placedObject:
                {
                    var placeableObject = placedObject.Base.TryResolve(param.LinkCache);
                    if (placeableObject is null) return;

                    if (isInvalid(placeableObject))
                    {
                        param.AddTopic(
                            topic.Format(placedObject.Base));
                    }
                    break;
                }
                case IPlacedNpcGetter placedNpcGetter:
                {
                    param.AddTopic(
                        topic.Format(placedNpcGetter.Base));
                    break;
                }
            }
        }

        void CheckNpc(Func<INpcGetter, bool> predicate, TopicDefinition<IFormLinkGetter> topic)
        {
            switch (placed)
            {
                case IPlacedArrowGetter placedGetter:
                {
                    param.AddTopic(
                        topic.Format(placedGetter.Projectile));
                    break;
                }
                case IPlacedBarrierGetter placedGetter:
                {
                    param.AddTopic(
                        topic.Format(placedGetter.Projectile));
                    break;
                }
                case IPlacedBeamGetter placedGetter:
                {
                    param.AddTopic(
                        topic.Format(placedGetter.Projectile));
                    break;
                }
                case IPlacedConeGetter placedGetter:
                {
                    param.AddTopic(
                        topic.Format(placedGetter.Projectile));
                    break;
                }
                case IPlacedFlameGetter placedGetter:
                {
                    param.AddTopic(
                        topic.Format(placedGetter.Projectile));
                    break;
                }
                case IPlacedHazardGetter placedGetter:
                {
                    param.AddTopic(
                        topic.Format(placedGetter.Hazard));
                    break;
                }
                case IPlacedMissileGetter placedGetter:
                {
                    param.AddTopic(
                        topic.Format(placedGetter.Projectile));
                    break;
                }
                case IPlacedTrapGetter placedGetter:
                {
                    param.AddTopic(
                        topic.Format(placedGetter.Projectile));
                    break;
                }
                case IPlacedObjectGetter placedObject:
                {
                    param.AddTopic(
                        topic.Format(placedObject.Base));
                    break;
                }
                case IPlacedNpcGetter placedNpcGetter:
                {
                    var npc = placedNpcGetter.Base.TryResolve(param.LinkCache);
                    if (npc is null) return;

                    if (predicate(npc))
                    {
                        param.AddTopic(
                            topic.Format(placedNpcGetter.Base));
                    }
                    break;
                }
            }
        }
    }

    public IEnumerable<Func<IPlacedGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.LocationRefTypes;
    }
}
