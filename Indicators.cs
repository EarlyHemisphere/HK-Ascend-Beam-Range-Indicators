using UnityEngine;
using Logger = Modding.Logger;
using ModCommon.Util;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;

namespace AscendBeamRangeIndicators {
    internal class Indicators : MonoBehaviour {
        private PlayMakerFSM attackChoices;
        private PlayMakerFSM attackCommands;
        private PlayMakerFSM ascendBeamControl = null;
        private GameObject knight;
        private GameObject eyeBeamGlow = null;
        private GameObject ascendBeam = null;
        private GameObject leftBound;
        private LineRenderer leftBoundRenderer;
        private GameObject rightBound;
        private LineRenderer rightBoundRenderer;
        private GameObject prevLeftBound;
        private LineRenderer prevLeftBoundRenderer;
        private GameObject prevRightBound;
        private LineRenderer prevRightBoundRenderer;
        private GameObject leftInnerBound;
        private LineRenderer leftInnerBoundRenderer;
        private GameObject rightInnerBound;
        private LineRenderer rightInnerBoundRenderer;
        private GameObject prevLeftInnerBound;
        private LineRenderer prevLeftInnerBoundRenderer;
        private GameObject prevRightInnerBound;
        private LineRenderer prevRightInnerBoundRenderer;
        private GameObject highestMissChance;
        private LineRenderer highestMissChanceRenderer;
        private float beamHalfWidth = 0.8828f;
        private float lineWidth = 0.15f;
        private float innerLineWidth = 0.05f;
        private bool linesInitialized = false;
        private bool ascensionCompleted = false;
        private void Awake() {
            Log("Added BeamRangeIndicators MonoBehaviour");

            attackCommands = gameObject.LocateMyFSM("Attack Commands");
            attackChoices = gameObject.LocateMyFSM("Attack Choices");
            knight = GameObject.Find("Knight");

            leftBound = new GameObject();
            leftBound.AddComponent<LineRenderer>();
            leftBoundRenderer = leftBound.GetComponent<LineRenderer>();
            createLine(leftBoundRenderer, Color.cyan);

            rightBound = new GameObject();
            rightBound.AddComponent<LineRenderer>();
            rightBoundRenderer = rightBound.GetComponent<LineRenderer>();
            createLine(rightBoundRenderer, Color.cyan);

            prevLeftBound = new GameObject();
            prevLeftBound.AddComponent<LineRenderer>();
            prevLeftBoundRenderer = prevLeftBound.GetComponent<LineRenderer>();
            createLine(prevLeftBoundRenderer, Color.red);

            prevRightBound = new GameObject();
            prevRightBound.AddComponent<LineRenderer>();
            prevRightBoundRenderer = prevRightBound.GetComponent<LineRenderer>();
            createLine(prevRightBoundRenderer, Color.red);

            leftInnerBound = new GameObject();
            leftInnerBound.AddComponent<LineRenderer>();
            leftInnerBoundRenderer = leftInnerBound.GetComponent<LineRenderer>();
            createLine(leftInnerBoundRenderer, Color.cyan, innerLineWidth);

            rightInnerBound = new GameObject();
            rightInnerBound.AddComponent<LineRenderer>();
            rightInnerBoundRenderer = rightInnerBound.GetComponent<LineRenderer>();
            createLine(rightInnerBoundRenderer, Color.cyan, innerLineWidth);

            prevLeftInnerBound = new GameObject();
            prevLeftInnerBound.AddComponent<LineRenderer>();
            prevLeftInnerBoundRenderer = prevLeftInnerBound.GetComponent<LineRenderer>();
            createLine(prevLeftInnerBoundRenderer, Color.red, innerLineWidth);

            prevRightInnerBound = new GameObject();
            prevRightInnerBound.AddComponent<LineRenderer>();
            prevRightInnerBoundRenderer = prevRightInnerBound.GetComponent<LineRenderer>();
            createLine(prevRightInnerBoundRenderer, Color.red, innerLineWidth);


            highestMissChance = new GameObject();
            highestMissChance.AddComponent<LineRenderer>();
            highestMissChanceRenderer = highestMissChance.GetComponent<LineRenderer>();
            createLine(highestMissChanceRenderer, Color.green);
        }

        private void Update() {
            try {
                if (ascendBeam == null) {
                    ascendBeam = GameObject.Find("Ascend Beam");
                }

                if (eyeBeamGlow == null) {
                    eyeBeamGlow = GameObject.Find("Eye Beam Glow");
                }

                if (attackChoices.ActiveStateName == "A2 End" &&
                    gameObject.transform.position.y >= 150f &&
                    eyeBeamGlow != null &&
                    ascendBeam != null &&
                    !ascensionCompleted) { // Ascension has begun and Radiance has started firing beams
                    if (knight.transform.position.y >= 153f) {
                        destroyLines();
                        ascensionCompleted = true;
                    } else {
                        if (ascendBeamControl == null) {
                            ascendBeamControl = ascendBeam.LocateMyFSM("Control");
                        }

                        if (!linesInitialized) {
                            Log("Ascension has begun, adding range indicators");
                            highestMissChanceRenderer.SetPosition(0, eyeBeamGlow.transform.position);
                            highestMissChanceRenderer.SetPosition(1, eyeBeamGlow.transform.position + new Vector3(0, -200, 0));
                            linesInitialized = true;

                            attackCommands.AddAction("Aim", new CallMethod {
                                behaviour = this,
                                methodName = "UpdatePreviousIndicators",
                                parameters = new FsmVar[0],
                                everyFrame = false,
                            });
                        }

                        updateIndicators();
                    }
                }
            } catch (System.NullReferenceException) {
                // Radiance is not firing beams yet
            } catch (System.Exception e) {
                Log("Could not draw indicators: " + e);
            }
        }

        private void createLine(LineRenderer renderer, Color color, float width) {
            renderer.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));;
            renderer.startColor = color;
            renderer.endColor = color;
            renderer.startWidth = width;
            renderer.endWidth = width;
        }

        private void createLine(LineRenderer renderer, Color color) {
            createLine(renderer, color, lineWidth);
        }

        private void updateIndicators() {
            Vector3 knightPos = knight.transform.position;
            Vector3 eyeBeamGlowPos = eyeBeamGlow.transform.position;

            // Calculate direction from center to knight (with -0.5 Y offset)
            Vector3 targetPos = knightPos - new Vector3(0, 0.5f, 0);
            Vector3 directionToKnight = (targetPos - eyeBeamGlowPos).normalized;
       
            // Apply the +5 and -5 degree rotations to the base direction
            Vector3 leftBeamDirection = Quaternion.Euler(0, 0, -5) * directionToKnight;
            Vector3 rightBeamDirection = Quaternion.Euler(0, 0, 5) * directionToKnight;
       
            // Calculate perpendicular offsets for the beam width
            // The perpendicular is 90 degrees rotated from the beam direction
            Vector3 leftPerpendicular = Quaternion.Euler(0, 0, 90) * leftBeamDirection;
            Vector3 rightPerpendicular = Quaternion.Euler(0, 0, 90) * rightBeamDirection;
       
            // Calculate the outer edge origins
            Vector3 leftOrigin = eyeBeamGlowPos - leftPerpendicular * (beamHalfWidth - lineWidth * 1.7f);
            Vector3 rightOrigin = eyeBeamGlowPos + rightPerpendicular * (beamHalfWidth - lineWidth * 1.4f);
            leftOrigin.z = rightOrigin.z = ascendBeam.transform.position.z;

            // Draw lines from origins along the rotated directions
            leftBoundRenderer.SetPosition(0, leftOrigin);
            rightBoundRenderer.SetPosition(0, rightOrigin);
            leftBoundRenderer.SetPosition(1, leftOrigin + leftBeamDirection * 400);
            rightBoundRenderer.SetPosition(1, rightOrigin + rightBeamDirection * 400);

            // Calculate inner boundaries (one beam width inward)
            Vector3 leftInnerOrigin = eyeBeamGlowPos + leftPerpendicular * (beamHalfWidth - innerLineWidth * 3.4f);
            Vector3 rightInnerOrigin = eyeBeamGlowPos - rightPerpendicular * (beamHalfWidth - innerLineWidth * 4.1f);
            leftInnerOrigin.z = rightInnerOrigin.z = ascendBeam.transform.position.z;

            leftInnerBoundRenderer.SetPosition(0, leftInnerOrigin);
            rightInnerBoundRenderer.SetPosition(0, rightInnerOrigin);
            leftInnerBoundRenderer.SetPosition(1, leftInnerOrigin + leftBeamDirection * 400);
            rightInnerBoundRenderer.SetPosition(1, rightInnerOrigin + rightBeamDirection * 400);
        }

        public void UpdatePreviousIndicators() {
            prevLeftBoundRenderer.SetPosition(0, leftBoundRenderer.GetPosition(0));
            prevLeftBoundRenderer.SetPosition(1, leftBoundRenderer.GetPosition(1));
            prevRightBoundRenderer.SetPosition(0, rightBoundRenderer.GetPosition(0));
            prevRightBoundRenderer.SetPosition(1, rightBoundRenderer.GetPosition(1));
            prevLeftInnerBoundRenderer.SetPosition(0, leftInnerBoundRenderer.GetPosition(0));
            prevLeftInnerBoundRenderer.SetPosition(1, leftInnerBoundRenderer.GetPosition(1));
            prevRightInnerBoundRenderer.SetPosition(0, rightInnerBoundRenderer.GetPosition(0));
            prevRightInnerBoundRenderer.SetPosition(1, rightInnerBoundRenderer.GetPosition(1));
        }

        private void destroyLines() {
            attackCommands.RemoveAction("Aim", 12);
            Destroy(leftBound);
            Destroy(rightBound);
            Destroy(prevLeftBound);
            Destroy(prevRightBound);
            Destroy(leftInnerBound);
            Destroy(rightInnerBound);
            Destroy(prevLeftInnerBound);
            Destroy(prevRightInnerBound);
            Destroy(highestMissChance);
            Destroy(this);
        }

        private static void Log(object obj) {
            Logger.Log("[AscendBeamRangeIndicators] - " + obj);
        }
    }
}
