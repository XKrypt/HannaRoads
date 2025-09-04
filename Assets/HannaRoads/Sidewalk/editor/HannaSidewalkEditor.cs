using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem;
using System.Linq;
using Codice.CM.Client.Gui;

namespace HannaRoads
{
    [CustomEditor(typeof(HannaSidewalk))]
    public class HannaSidewalkEditor : Editor
    {

        HannaSidewalk hannaSideWalk;
        SideWalkSegment currentSSegment;
        RoadMode roadMode = RoadMode.Segment;


        void OnEnable()
        {
            // Desativa a seleção de objetos na Unity
            hannaSideWalk = target as HannaSidewalk;
        }

        public GUIStyle TitleStyle(int fontSize = 16)
        {

            return new GUIStyle(EditorStyles.label)
            {
                fontStyle = FontStyle.Bold,
                fontSize = fontSize
            };
        }

        public override void OnInspectorGUI()
        {

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Road mode :" + roadMode.ToString(), TitleStyle());
          

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(1);
            EditorGUILayout.Space(5);

            hannaSideWalk.activeSegment = (SideWalkSegment)EditorGUILayout.ObjectField("Active segment", hannaSideWalk.activeSegment, typeof(SideWalkSegment), true);
            hannaSideWalk.curbstoneDefaultMaterial = (Material)EditorGUILayout.ObjectField("Default Curbstone Material", hannaSideWalk.curbstoneDefaultMaterial, typeof(Material), true);
            hannaSideWalk.sidewalkDefaultMaterial = (Material)EditorGUILayout.ObjectField("Default Sidewalk Material", hannaSideWalk.sidewalkDefaultMaterial, typeof(Material), true);


            EditorGUILayout.LabelField("Hold Shift: Start position road.");
            EditorGUILayout.LabelField("Shift + A : Change mode.");
            EditorGUILayout.LabelField("Shift + Alt : Create disconnected.");
            EditorGUILayout.LabelField("Shift + S : Create intersection on start of the road.");
            EditorGUILayout.LabelField("Shift + E : Create intersection on end of road.");

            serializedObject.ApplyModifiedProperties();
        }


        [ContextMenu("HannaRoads/Clear2")]
        public void Clear()
        {
            hannaSideWalk.referencePoints.Clear();
            hannaSideWalk.Clear();
        }

        private void SwitchMode()
        {
            roadMode = roadMode == 0 ? RoadMode.Intersection : RoadMode.Segment;
        }
        private void OnSceneGUI()
        {

            Event frameEvent = Event.current;



            //Detecta a tecla shift
            if (frameEvent.shift)
            {

                // Deixa o game object do script ativo
                Selection.activeObject = hannaSideWalk.gameObject;


                //Seleciona um ponto no espaço 3d baseado na posição do mouse usando raycast
                Ray ray = HandleUtility.GUIPointToWorldRay(frameEvent.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 1000.0f))
                {

                    if (frameEvent.alt)
                    {
                        Handles.color = Color.green;
                    }
                    else
                    {
                        Handles.color = Color.grey;
                    }
                    //Cria um pequeno cubo que mostra onde a estrada deve começar ou terminar se ja estiver seguindo outra estrada
                    Handles.DrawWireCube(hit.point, Vector3.one * 0.1f);

                    if (frameEvent.alt)
                    {
                        Handles.color = Color.gray;
                    }
                    else
                    {
                        Handles.color = Color.white;
                    }


                    //Executa conforme o mouse se move
                    if (frameEvent.type == EventType.MouseMove)
                    {

                        //Atualiza a renderização da cena enquanto estiver posicionando estrada
                        EditorUtility.SetDirty(target);
                        SceneView.RepaintAll();
                    }

                    //Se o botão direito for pressionado
                    if (frameEvent.type == EventType.MouseDown && frameEvent.button == 0 && currentSSegment == null)
                    {

                        //Se já existir segmentos
                        if (hannaSideWalk.sSegments.Count > 0 && !frameEvent.alt)
                        {



                            if (hannaSideWalk.activeSegment == null)
                            {
                                hannaSideWalk.activeSegment = hannaSideWalk.sSegments.Last();
                            }
                            //Posiciona uma nova estrada no final da anterior
                            currentSSegment = hannaSideWalk.SpawnRoad(hannaSideWalk.activeSegment.end.position);

                        }
                        else
                        {
                            //Simplesmente spawn uma nova estrada no ponto em que o mouse estiver posicionado
                            currentSSegment = hannaSideWalk.SpawnRoad(hit.point);

                        }


                        SideWalkReferencePoint[] referencePoints = GenerateReferencePointsForRoad(hit, frameEvent);
                        currentSSegment.controlPoints[0].referencePoint = currentSSegment.startRef;
                        currentSSegment.controlPoints[1].referencePoint = currentSSegment.endRef;



                        if (hannaSideWalk.sSegments.Count < 2 && !frameEvent.alt)
                        {
                            //Posiciona o ponto de referencia um pouco acima do ponto para que não haja problemas de renderização
                            referencePoints[0].transform.position = hit.point + Vector3.up * 0.001f; ;
                        }

                        EditorUtility.SetDirty(target);
                        SceneView.RepaintAll();
                    }
                    else if (currentSSegment != null && frameEvent.type != EventType.MouseDown && frameEvent.button != 1)
                    //Executa enquanto o shift esta pressionado e atualiza o final da estrada junto com o mouse se uma estrada foi criada
                    {
                        currentSSegment.endRef.transform.position = hit.point + Vector3.up * 0.001f;
                        currentSSegment.controlPoints[0].transform.position = Vector3.Lerp(currentSSegment.start.transform.position, currentSSegment.end.transform.position, 0.1f) + Vector3.up * 0.001f;
                        currentSSegment.controlPoints[1].transform.position = Vector3.Lerp(currentSSegment.end.transform.position, currentSSegment.start.transform.position, 0.1f) + Vector3.up * 0.001f;


                      


                    }
                    else if (frameEvent.type == EventType.MouseDown && frameEvent.button == 0)
                    {
                        //Posiciona a estrada definitivamente
                        currentSSegment.Generate();
                        hannaSideWalk.activeSegment = currentSSegment;
                        currentSSegment = null;
                        EditorUtility.SetDirty(target);


                    }
                }
            }
            else
            {
                //Limpa caso seja cancelada
                if (currentSSegment != null)
                {
                    DestroyImmediate(currentSSegment.gameObject);
                }
            }
        }

        SideWalkReferencePoint[] GenerateReferencePointsForRoad(RaycastHit hit, Event e)
        {

            SideWalkReferencePoint[] referencePoints = new SideWalkReferencePoint[2];

            if (hannaSideWalk.sSegments.Count > 1 && !e.alt)
            {

                //Se ja existir segmentos antes do que ja foi criado
                //Cria um ponto de referência final do anterior como inicio para o novo
                SideWalkReferencePoint startReferencePoint = hannaSideWalk.CreateReferencePoint(null, currentSSegment);
                hannaSideWalk.referencePoints.Add(startReferencePoint);
                SideWalkReferencePoint activeEndReferencePoint = hannaSideWalk.activeSegment.endRef;

                currentSSegment.endRef = hannaSideWalk.referencePoints.Last();
                currentSSegment.startRef = activeEndReferencePoint;
                currentSSegment.endRef.transform.SetParent(hannaSideWalk.transform);

                activeEndReferencePoint.sSegment = currentSSegment;

                referencePoints[0] = startReferencePoint;
                referencePoints[1] = activeEndReferencePoint;

                return referencePoints;


            }
            else
            {

                //Se não existir cria dois pontos de referencia um para o começo e outro para o final

                SideWalkReferencePoint startReferencePoint = hannaSideWalk.CreateReferencePoint(currentSSegment);
                SideWalkReferencePoint endReferencePoint = hannaSideWalk.CreateReferencePoint(null, currentSSegment);


                hannaSideWalk.referencePoints.Add(startReferencePoint);
                hannaSideWalk.referencePoints.Add(endReferencePoint);


                startReferencePoint.transform.SetParent(hannaSideWalk.transform);
                endReferencePoint.transform.SetParent(hannaSideWalk.transform);

                //Posiciona o ponto inicial na posição do mouse
                startReferencePoint.transform.position = hit.point;

                currentSSegment.startRef = startReferencePoint;
                currentSSegment.endRef = endReferencePoint;


                referencePoints[0] = startReferencePoint;
                referencePoints[1] = endReferencePoint;

                return referencePoints;
            }
        }




        void OnDisable()
        {
            // Restaura a seleção de objetos na Unity ao desativar o editor customizado
            Selection.activeObject = null;
        }
    }
}



