using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem;
using System.Linq;
using Codice.CM.Client.Gui;

namespace HannaRoads
{
    [CustomEditor(typeof(HannaRoad))]
    public class HannaRoadsEditor : Editor
    {

        HannaRoad hannaRoad;
        RSegment currentRSegment;
        RoadMode roadMode = RoadMode.Segment;


        void OnEnable()
        {
            // Desativa a seleção de objetos na Unity
            hannaRoad = target as HannaRoad;
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
            if (hannaRoad.activeSegment != null && hannaRoad.activeSegment.endIntersection == null)
            {
                if (GUILayout.Button("Change mode"))
                {
                    SwitchMode();
                }
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(1);
            if (GUILayout.Button("Align all roads with terrain"))
            {
                hannaRoad.AlignTerrainForAllSegments();
            }
            EditorGUILayout.Space(5);

            hannaRoad.activeSegment = (RSegment)EditorGUILayout.ObjectField("Active segment", hannaRoad.activeSegment, typeof(RSegment), true);
            hannaRoad.activeIntersection = (HannaIntersection)EditorGUILayout.ObjectField("Active intersection", hannaRoad.activeIntersection, typeof(HannaIntersection), true);


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
            hannaRoad.referencePoints.Clear();
            hannaRoad.Clear();
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
                Selection.activeObject = hannaRoad.gameObject;

                if (frameEvent.keyCode == KeyCode.A && frameEvent.type == EventType.KeyDown && hannaRoad.activeSegment.endIntersection == null)
                {
                    roadMode = roadMode == 0 ? RoadMode.Intersection : RoadMode.Segment;
                    EditorUtility.SetDirty(target);
                }

                if (hannaRoad.activeSegment != null && hannaRoad.activeSegment.endIntersection != null && roadMode == RoadMode.Segment)
                {
                    roadMode = RoadMode.Intersection;
                    EditorUtility.SetDirty(target);
                }




                //Seleciona um ponto no espaço 3d baseado na posição do mouse usando raycast
                Ray ray = HandleUtility.GUIPointToWorldRay(frameEvent.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 1000.0f))
                {


                    if (hannaRoad.activeSegment != null)
                    {
                        if (frameEvent.keyCode == KeyCode.S && frameEvent.type == EventType.KeyDown

                        && hannaRoad.activeSegment.startRef.previousRSegment == null
                        && hannaRoad.activeSegment.endIntersection == null
                        )
                        {
                            if (hannaRoad.activeSegment != null)
                            {
                                hannaRoad.activeSegment.startRef.CreateIntersection();
                            }
                        }
                        else if (frameEvent.keyCode == KeyCode.E && frameEvent.type == EventType.KeyDown
                        && hannaRoad.activeSegment.endRef.rSegment == null
                        && hannaRoad.activeSegment.endIntersection == null
                        )
                        {
                            if (hannaRoad.activeSegment != null)
                            {
                                hannaRoad.activeSegment.endRef.CreateIntersection();

                            }
                        }
                    }
                    else if ((frameEvent.keyCode == KeyCode.S || frameEvent.keyCode == KeyCode.E) && frameEvent.type == EventType.KeyDown)
                    {
                        HannaIntersection intersection = new GameObject().AddComponent<HannaIntersection>();

                        intersection.extrusionSize = 1f;
                        intersection.size = 0.5f;
                        intersection.shape = 0f;
                        intersection.transform.position = hit.point + Vector3.up * 0.001f;
                        intersection.gameObject.name = "Intersection";
                        intersection.Generate();


                    }

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
                    if (frameEvent.type == EventType.MouseDown && frameEvent.button == 0 && currentRSegment == null)
                    {

                        //Se já existir segmentos
                        if (hannaRoad.rSegments.Count > 0 && !frameEvent.alt)
                        {

                            if (hannaRoad.activeIntersection != null && roadMode == RoadMode.Intersection)
                            {
                                Undo.RecordObject(null, "Descrição da ação");
                                Vector3 spawnPosition = hannaRoad.activeIntersection.transform.TransformPoint(hannaRoad.activeIntersection.intersectionPoints[1].middlePoint);
                                currentRSegment = hannaRoad.SpawnRoad(spawnPosition);


                                currentRSegment.startIntersection = hannaRoad.activeIntersection;
                            }
                            else
                            {
                                if (hannaRoad.activeSegment == null)
                                {
                                    hannaRoad.activeSegment = hannaRoad.rSegments.Last();
                                }
                                //Posiciona uma nova estrada no final da anterior
                                currentRSegment = hannaRoad.SpawnRoad(hannaRoad.activeSegment.end.position);
                            }
                        }
                        else
                        {
                            //Simplesmente spawn uma nova estrada no ponto em que o mouse estiver posicionado
                            currentRSegment = hannaRoad.SpawnRoad(hit.point);

                        }


                        ReferencePoint[] referencePoints = GenerateReferencePointsForRoad(currentRSegment.startIntersection != null, hit, frameEvent);
                        currentRSegment.controlPoints[0].referencePoint = currentRSegment.startRef;
                        currentRSegment.controlPoints[1].referencePoint = currentRSegment.endRef;



                        if (hannaRoad.rSegments.Count < 2 && !frameEvent.alt)
                        {
                            //Posiciona o ponto de referencia um pouco acima do ponto para que não haja problemas de renderização
                            referencePoints[0].transform.position = hit.point + Vector3.up * 0.001f; ;
                        }

                        EditorUtility.SetDirty(target);
                        SceneView.RepaintAll();
                    }
                    else if (currentRSegment != null && frameEvent.type != EventType.MouseDown && frameEvent.button != 1)
                    //Executa enquanto o shift esta pressionado e atualiza o final da estrada junto com o mouse se uma estrada foi criada
                    {
                        currentRSegment.endRef.transform.position = hit.point + Vector3.up * 0.001f;
                        currentRSegment.controlPoints[0].transform.position = Vector3.Lerp(currentRSegment.start.transform.position, currentRSegment.end.transform.position, 0.1f) + Vector3.up * 0.001f;
                        currentRSegment.controlPoints[1].transform.position = Vector3.Lerp(currentRSegment.end.transform.position, currentRSegment.start.transform.position, 0.1f) + Vector3.up * 0.001f;


                        if (frameEvent.type == EventType.ScrollWheel && currentRSegment.startRef.intersectionAttachment != null)
                        {
                            ReferencePoint referencePoint = currentRSegment.startRef;
                            int intersectionConnectionIndex = referencePoint.interIndex + (frameEvent.delta.x > 0 ? 1 : -1);
                            int max = referencePoint.intersectionAttachment.crossing4 ? 3 : 2;
                            if (intersectionConnectionIndex > max)
                            {
                                intersectionConnectionIndex = 0;
                            }
                            else if (intersectionConnectionIndex < 0)
                            {
                                intersectionConnectionIndex = max;
                            }
                            referencePoint.interIndex = intersectionConnectionIndex;

                            referencePoint.UpdateReference();
                            referencePoint.UpdateMeshVerts();

                            Debug.Log("Scroll: " + frameEvent.delta.x);
                        }



                        if (currentRSegment.startRef.previousRSegment != null)
                        {
                            currentRSegment.startRef.UpdateMeshVerts();
                        }


                    }
                    else if (frameEvent.type == EventType.MouseDown && frameEvent.button == 0)
                    {
                        //Posiciona a estrada definitivamente
                        currentRSegment.Generate();
                        hannaRoad.activeSegment = currentRSegment;
                        currentRSegment = null;
                        EditorUtility.SetDirty(target);


                    }
                }
            }
            else
            {
                //Limpa caso seja cancelada
                if (currentRSegment != null)
                {
                    DestroyImmediate(currentRSegment.gameObject);
                }
            }
        }

        ReferencePoint[] GenerateReferencePointsForRoad(bool intersection, RaycastHit hit, Event e)
        {

            ReferencePoint[] referencePoints = new ReferencePoint[2];

            if (intersection)
            {

                Debug.Log("Criando a partir de intersecção");
                ReferencePoint startReferencePoint = hannaRoad.CreateReferencePoint(currentRSegment);
                ReferencePoint endReferencePoint = hannaRoad.CreateReferencePoint(null, currentRSegment);




                // cria dois pontos de referencia um para o começo e outro para o final
                hannaRoad.referencePoints.Add(startReferencePoint);
                hannaRoad.referencePoints.Add(endReferencePoint);




                startReferencePoint.transform.SetParent(hannaRoad.transform);
                endReferencePoint.transform.SetParent(hannaRoad.transform);

                //Posiciona o ponto inicial na posição do mouse
                Vector3 interAttachPos = hannaRoad.activeIntersection.transform.TransformPoint(currentRSegment.startIntersection.intersectionPoints[0].middlePoint);
                startReferencePoint.transform.position = interAttachPos;

                currentRSegment.startRef = startReferencePoint;
                currentRSegment.endRef = endReferencePoint;


                startReferencePoint.intersectionAttachment = hannaRoad.activeIntersection;
                referencePoints[0] = startReferencePoint;
                referencePoints[1] = endReferencePoint;

                return referencePoints;
            }

            if (hannaRoad.rSegments.Count > 1 && !e.alt)
            {

                //Se ja existir segmentos antes do que ja foi criado
                //Cria um ponto de referência final do anterior como inicio para o novo
                ReferencePoint startReferencePoint = hannaRoad.CreateReferencePoint(null, currentRSegment);
                hannaRoad.referencePoints.Add(startReferencePoint);
                ReferencePoint activeEndReferencePoint = hannaRoad.activeSegment.endRef;

                currentRSegment.endRef = hannaRoad.referencePoints.Last();
                currentRSegment.startRef = activeEndReferencePoint;
                currentRSegment.endRef.transform.SetParent(hannaRoad.transform);

                activeEndReferencePoint.rSegment = currentRSegment;

                referencePoints[0] = startReferencePoint;
                referencePoints[1] = activeEndReferencePoint;

                return referencePoints;


            }
            else
            {

                //Se não existir cria dois pontos de referencia um para o começo e outro para o final

                ReferencePoint startReferencePoint = hannaRoad.CreateReferencePoint(currentRSegment);
                ReferencePoint endReferencePoint = hannaRoad.CreateReferencePoint(null, currentRSegment);


                hannaRoad.referencePoints.Add(startReferencePoint);
                hannaRoad.referencePoints.Add(endReferencePoint);


                startReferencePoint.transform.SetParent(hannaRoad.transform);
                endReferencePoint.transform.SetParent(hannaRoad.transform);

                //Posiciona o ponto inicial na posição do mouse
                startReferencePoint.transform.position = hit.point;

                currentRSegment.startRef = startReferencePoint;
                currentRSegment.endRef = endReferencePoint;


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



    enum RoadMode
    {
        Segment,
        Intersection
    }
}



