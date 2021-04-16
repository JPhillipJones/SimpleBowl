using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleBowl.Models
{
    public class BowlingGame
    {
        public string state { get; set; } = "start";
        public int score { get; set; }
        public string scoreHeader { get; set; }
        public string display { get; set; }
        public int currentFrame { get; set; }
        public int currentRoll { get; set; }
        public List<Frame> frames { get; set; }

        public Ball ball { get; set; }
        public List<Pin> pins { get; set; }

        public BowlingGame()
            {
                setUp();
            }

        public void incrementFrame()
        {
            if (currentFrame < 10) currentFrame++;
            currentRoll = 1;
        
        }
        public void incrementRoll()
        {
            if(currentRoll < 2)
            {
                currentRoll++;
            }
            else
            {
                if (currentFrame == 10 && currentRoll < 3) currentRoll++;
            }
        }
  
        public Frame getCurrentFrame()
        {
            return frames.Where(f => f.number == currentFrame).FirstOrDefault();
        }

        public void setUp()
        {
            score = 0;
            scoreHeader = "Score: ";
            display = "-Click to Start-";
            currentFrame = 1;
            currentRoll = 1;
            resetBall();
            resetFrames();
            setupPins();
         }
        public void resetFrames()
        {
            frames = new List<Frame>();
            for(int i = 1; i <= 10; i++)
            {
                frames.Add(new Frame(i));
            }
        }
        public void setupPins()
        {
            pins = new List<Pin>();
            for (int i = 1; i <= 10; i++)
            {
                pins.Add(new Pin());
            }
            for (int i = 0; i < 10; i++)
            {
                    if (i < 4) pins[i].Y = 25;                        
                    if (i >= 4 && i < 7) pins[i].Y = 55;                       
                    if (i >= 7 && i < 9) pins[i].Y = 90;                    
                    if (i >= 9) pins[i].Y = 125;                                  
            }
            pins[0].X = 25; pins[1].X = 75; pins[2].X = 125; pins[3].X = 175;
            pins[4].X = 50; pins[5].X = 100; pins[6].X = 150;
            pins[7].X = 75; pins[8].X = 125;
            pins[9].X = 100;
        }
        public void resetPins()
        {
            foreach(var p in pins)
            {
                p.upRight = true;
            }
        }
        public void resetBall()
        {
            ball = new Ball();
        }

        public void calculateScore()
        {            
            score = 0;
            //total pins first
            for (int i = 0; i < frames.Count; i++)
            {
                var f = frames[i];
                f.score = f.rolls.Sum(r => r.score);
                //Add values from subsequent frames if we are not on the last one since those rolls are simply summed
                if (f.number < 10)
                {
                    //spare        
                    if (f.isSpare())
                    {
                        f.score += getNextRoll(i).score;
                    }

                    //strike
                    if (f.isStrike())
                    {
                        f.score += getNextRoll(i).score;
                        f.score += getRollAfterNext(i).score;
                    }
                }

                score += f.score;
            }
       
        }
        public void advanceFrameAndRoll()
        {            
              //FRAMES 1-9 Scenarios
                          //strike
            if (!isTenthFrame() && isFirstRoll() && getCurrentFrame().isStrike())
            {
                getCurrentFrame().frameType = "strike";
                incrementFrame();
                resetPins();               
                return;
            }
            //non-strike
            if (!isTenthFrame() && isFirstRoll() && !getCurrentFrame().isStrike())
            {
                incrementRoll();
                return;
            }
            //spare
            if (!isTenthFrame() && isSecondRoll() && getCurrentFrame().isSpare())
            {
                getCurrentFrame().frameType = "spare";
                incrementFrame();
                resetPins();                
                return;
            }
            //non-spare
            if (!isTenthFrame() && isSecondRoll() && !getCurrentFrame().isSpare())
            {
                incrementFrame();
                resetPins();                
                return;
            }

            //TENTH FRAME Scenarios
            //strike
            if (isTenthFrame() && isFirstRoll() && getCurrentFrame().isStrike())
            {
                getCurrentFrame().frameType = "strike";
                resetPins();
                incrementRoll();
                return;
            }
            //non-strike
            if (isTenthFrame() && isFirstRoll() && !getCurrentFrame().isStrike())
            {
                incrementRoll();               
                return;
            }
            //double-strike
            if (isTenthFrame() && isSecondRoll() && getCurrentFrame().isDoubleStrike())
            {
                resetPins();
                incrementRoll();                
                return;
            }
            //spare
            if (isTenthFrame() && isSecondRoll() && getCurrentFrame().isSpare())
            {
                getCurrentFrame().frameType = "spare";
                resetPins();
                incrementRoll();                
                return;
            }
            //first roll was strike but not second
            if (isTenthFrame() && isSecondRoll() && getCurrentFrame().isStrike() && !getCurrentFrame().isDoubleStrike())
            {
                incrementRoll();            
                return;
            }
            //non-spare or strike on first roll
            if (isTenthFrame() && isSecondRoll() && !getCurrentFrame().isSpare() && !getCurrentFrame().isStrike())
            {
                gameOver();
                return;
            }
            //Third Roll
            if (isTenthFrame() && isThirdRoll())
            {
                gameOver();
                return;
            }

        }
        private void gameOver()
            {
                state = "over";
                display = "Game Finished!";
                scoreHeader = "Final Score: ";
            }

        private Roll getNextRoll(int index)
        {
            return frames[index + 1].rolls[0];
        }

        private Roll getRollAfterNext(int index)
        {
            
            if (frames[index + 1].isStrike() && index < 8)
            {
                return frames[index + 2].rolls[0];
            }
            else
            {
                return frames[index + 1].rolls[1];
            }

        }

        public bool isFirstRoll()
        {
            return currentRoll == 1;
        }

        public bool isSecondRoll()
        {
            return currentRoll == 2;
        }

        public bool isThirdRoll()
        {
            return currentRoll ==3;
        }

        public bool isTenthFrame()
        {
            return currentFrame == 10;
        }
    }


    public class Frame
    {
        public int number { get; set; }

        public int score { get; set; }

        public string frameType { get; set; }

        public List<Roll> rolls {get;set;}

        public Frame(int nmbr)
        {
            number = nmbr;
            score = 0;
            rolls = new List<Roll>();
            rolls.Add(new Roll(1));
            rolls.Add(new Roll(2));
            frameType = "split";
            if (nmbr == 10) rolls.Add(new Roll(3));
        }

        public bool isSpare()
        {
            return (rolls[0].score < 10 && rolls.Sum(r => r.score) > 9);
        }

        public bool isStrike()
        {
            return (rolls[0].score > 9);
        }
        public bool isDoubleStrike()
        {
            return (rolls[0].score > 9 && rolls[1].score > 9);
        }

    }

    public class Roll
    {
        public int number { get; set; }
        public int score { get; set; }
        public Roll(int nmbr)
        {
            number = nmbr;
            score = 0;
        }
    }

    public class Pin
    {
        public int X { get; set; }
        public int Y { get; set; }
        public bool upRight { get; set; } = true;
    }

    public class Ball
    {
        public int X { get; set; } = 100;
        public int Y { get; set; } = 460;
        public bool rolling { get; set; } = false;
        public int ocillate { get; set; } = 1;
    }
}
