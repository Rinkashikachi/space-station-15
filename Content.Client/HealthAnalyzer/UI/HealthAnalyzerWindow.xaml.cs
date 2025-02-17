using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Disease.Components;
using Content.Shared.FixedPoint;
using Content.Shared.IdentityManagement;
using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface.CustomControls;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Prototypes;
using System.Text;
using static Content.Shared.MedicalScanner.SharedHealthAnalyzerComponent;

namespace Content.Client.HealthAnalyzer.UI
{
    [GenerateTypedNameReferences]
    public sealed partial class HealthAnalyzerWindow : DefaultWindow
    {
        public HealthAnalyzerWindow()
        {
            RobustXamlLoader.Load(this);
        }

        public void Populate(HealthAnalyzerScannedUserMessage msg)
        {
            var text = new StringBuilder();
            var entities = IoCManager.Resolve<IEntityManager>();

            if (msg.TargetEntity != null && entities.TryGetComponent<DamageableComponent>(msg.TargetEntity, out var damageable))
            {
                string entityName = "Unknown";
                if (msg.TargetEntity != null &&
                    entities.TryGetComponent<MetaDataComponent>(msg.TargetEntity.Value, out var metaData))
                    entityName = Identity.Name(msg.TargetEntity.Value, entities);

                IReadOnlyDictionary<string, FixedPoint2> DamagePerGroup = damageable.DamagePerGroup;
                IReadOnlyDictionary<string, FixedPoint2> DamagePerType = damageable.Damage.DamageDict;

                text.Append($"{Loc.GetString("health-analyzer-window-entity-health-text", ("entityName", entityName))}\n");

                /// Status Effects / Components
                if (entities.HasComponent<DiseasedComponent>(msg.TargetEntity))
                {
                    text.Append($"{Loc.GetString("disease-scanner-diseased")}\n");
                }else
                {
                    text.Append($"{Loc.GetString("disease-scanner-not-diseased")}\n");
                }

                /// Damage
                text.Append($"\n{Loc.GetString("health-analyzer-window-entity-damage-total-text", ("amount", damageable.TotalDamage))}\n");

                HashSet<string> shownTypes = new();

                var protos = IoCManager.Resolve<IPrototypeManager>();

                // Show the total damage and type breakdown for each damage group.
                foreach (var (damageGroupId, damageAmount) in DamagePerGroup)
                {
                    text.Append($"\n{Loc.GetString("health-analyzer-window-damage-group-text", ("damageGroup", damageGroupId), ("amount", damageAmount))}");
                    // Show the damage for each type in that group.
                    var group = protos.Index<DamageGroupPrototype>(damageGroupId);
                    foreach (var type in group.DamageTypes)
                    {
                        if (DamagePerType.TryGetValue(type, out var typeAmount))
                        {
                            // If damage types are allowed to belong to more than one damage group, they may appear twice here. Mark them as duplicate.
                            if (!shownTypes.Contains(type))
                            {
                                shownTypes.Add(type);
                                text.Append($"\n- {Loc.GetString("health-analyzer-window-damage-type-text", ("damageType", type), ("amount", typeAmount))}");
                            }
                        }
                    }
                    text.AppendLine();
                }
                Diagnostics.Text = text.ToString();
                SetSize = (250, 600);
            }
            else
            {
                Diagnostics.Text = Loc.GetString("health-analyzer-window-no-patient-data-text");
                SetSize = (250, 100);
            }
        }
    }
}
