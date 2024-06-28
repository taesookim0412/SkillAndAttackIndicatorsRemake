using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.EventSystems;
using UnityEngine;
using Assets.Crafter.Components.Models;
using DTT.AreaOfEffectRegions;

namespace Assets.DTT.Area_of_Effect_Regions.Demo.Interactive_Demo.Scripts.Observer
{
    public class SkillAndAttackIndicatorObserverProps
    {
        public SkillAndAttackIndicatorSystem SkillAndAttackIndicatorSystem;

        public ObserverUpdateProps ObserverUpdateProps;
        public SkillAndAttackIndicatorObserverProps(SkillAndAttackIndicatorSystem skillAndAttackIndicatorSystem, ObserverUpdateProps observerUpdateProps)
        {
            SkillAndAttackIndicatorSystem = skillAndAttackIndicatorSystem;
            ObserverUpdateProps = observerUpdateProps;
        }
    }
    public class SkillAndAttackIndicatorObserver
    {
        public static readonly string[] AbilityProjectorTypeNames = Enum.GetNames(typeof(AbilityProjectorType));
        public static readonly int AbilityProjectorTypeNamesLength = AbilityProjectorTypeNames.Length;
        public static readonly string[] AbilityProjectorMaterialTypeNames = Enum.GetNames(typeof(AbilityProjectorMaterialType));
        public static readonly int AbilityProjectorMaterialTypeNamesLength = AbilityProjectorMaterialTypeNames.Length;

        // The orthographic length is based on a radius so it is half the desired length.
        // Then, it must be multiplied by half again because it is a "half-length" in the documentation.
        private static readonly float OrthographicRadiusHalfDivMult = 1 / 4f;

        private SkillAndAttackIndicatorObserverProps Props;

        public ObserverStatus ObserverStatus = ObserverStatus.Active;
        
        public readonly AbilityProjectorType AbilityProjectorType;
        public readonly AbilityProjectorMaterialType AbilityProjectorMaterialType;
        public readonly AbilityIndicatorCastType AbilityIndicatorCastType;

        private bool ProjectorSet = false;
        private PoolBagDco<MonoBehaviour> InstancePool;
        private MonoBehaviour ProjectorMonoBehaviour;
        private GameObject ProjectorGameObject;

        private long LastTickTime;
        private long ElapsedTime;

        public SkillAndAttackIndicatorObserver(AbilityProjectorType abilityProjectorType,
            AbilityProjectorMaterialType abilityProjectorMaterialType, AbilityIndicatorCastType abilityIndicatorCastType,
            SkillAndAttackIndicatorObserverProps skillAndAttackIndicatorObserverProps
            )
        {
            AbilityProjectorType = abilityProjectorType;
            AbilityProjectorMaterialType = abilityProjectorMaterialType;
            AbilityIndicatorCastType = abilityIndicatorCastType;

            Props = skillAndAttackIndicatorObserverProps;
        }

        public void OnUpdate()
        {
            if (!ProjectorSet)
            {
                if (Props.SkillAndAttackIndicatorSystem.ProjectorInstancePools.TryGetValue(AbilityProjectorType, out var abilityMaterialTypesDict) &&
                    abilityMaterialTypesDict.TryGetValue(AbilityProjectorMaterialType, out InstancePool))
                {
                    // 3 texture option indices.
                    ProjectorMonoBehaviour = InstancePool.InstantiatePooled(null);
                    ProjectorGameObject = ProjectorMonoBehaviour.gameObject;

                    switch (AbilityProjectorType)
                    {
                        case AbilityProjectorType.Arc:
                            ArcRegionProjector arcRegionProjector = ProjectorGameObject.GetComponent<ArcRegionProjector>();
                            arcRegionProjector.Radius = 70;
                            arcRegionProjector.SetIgnoreLayers(Props.SkillAndAttackIndicatorSystem.ProjectorIgnoreLayersMask);

                            arcRegionProjector.UpdateProjectors();
                            break;
                        case AbilityProjectorType.Circle:
                            CircleRegionProjector circleRegionProjector = ProjectorGameObject.GetComponent<CircleRegionProjector>();
                            circleRegionProjector.Radius = 70;

                            circleRegionProjector.SetIgnoreLayers(Props.SkillAndAttackIndicatorSystem.ProjectorIgnoreLayersMask);
                            circleRegionProjector.UpdateProjectors();
                            break;
                        case AbilityProjectorType.Line:
                            LineRegionProjector lineRegionProjector = ProjectorGameObject.GetComponent<LineRegionProjector>();

                            // hard coded length units.
                            int lineLengthUnits = 25;
                            // multiply it by the orthographicRadiusHalfDivMultiplier
                            float orthographicLength = lineLengthUnits * OrthographicRadiusHalfDivMult;
                            lineRegionProjector.Length = orthographicLength;

                            lineRegionProjector.SetIgnoreLayers(Props.SkillAndAttackIndicatorSystem.ProjectorIgnoreLayersMask);
                            lineRegionProjector.UpdateProjectors();
                            break;
                        case AbilityProjectorType.ScatterLine:
                            ScatterLineRegionProjector scatterLineRegionProjector = ProjectorGameObject.GetComponent<ScatterLineRegionProjector>();
                            scatterLineRegionProjector.Length = 70;
                            scatterLineRegionProjector.Add(3);
                            scatterLineRegionProjector.UpdateLines();

                            // SetIgnoreLayers not supported with the current scatterlineregionprojector...
                            break;
                    }

                    ProjectorSet = true;
                }
                else
                {
                    ObserverStatus = ObserverStatus.Remove;
                    return;
                }
            }

            ElapsedTime += Props.ObserverUpdateProps.UpdateTickTimeDeltaTime;

            Vector3 terrainPosition = GetTerrainPosition();
            ProjectorGameObject.transform.position = terrainPosition;


            if (ElapsedTime > 5000L)
            {
                InstancePool.ReturnPooled(ProjectorMonoBehaviour);
                ObserverStatus = ObserverStatus.Remove;
            }
        }
        private Vector3 GetTerrainPosition()
        {
            //if (EventSystem.current.IsPointerOverGameObject() || !_isMouseOverGameWindow || IsPointerOverUIElement(GetEventSystemRaycastResults()))
            //    return _anchorPoint.transform.position;
            Ray ray = Props.SkillAndAttackIndicatorSystem.Camera.ScreenPointToRay(Input.mousePosition);
            return Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, Props.SkillAndAttackIndicatorSystem.TerrainLayer) ? 
                hit.point + new Vector3(0, 50, 0) 
                : new Vector3(0, 50, 0);
        }
        public void TriggerDoubleCast()
        {
            throw new NotImplementedException();
        }
    }

    public enum ObserverStatus
    {
        Active,
        Remove
    }
    public enum AbilityProjectorType
    {
        Arc,
        Circle,
        Line,
        ScatterLine,
    }
    public enum AbilityProjectorMaterialType
    {
        First,
        Second,
        Third
    }
    public enum AbilityIndicatorCastType
    {
        ShowDuringCast,
        DoubleCast
    }
}
