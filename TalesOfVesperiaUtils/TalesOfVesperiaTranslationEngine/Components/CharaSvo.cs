﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TalesOfVesperiaTranslationEngine.Components
{
	public class CharaSvo : PatcherComponent
	{
		public CharaSvo(Patcher Patcher)
			: base(Patcher)
		{
		}

        public void Handle()
        {
            Patcher.ProgressHandler.ExecuteActionsWithProgressTracking("chara.svo",
                Handle1
            );
        }

		private void Handle1()
		{
            Patcher.ProgressHandler.AddProgressLevel("chara.svo", 3, () =>
            {
                Patcher.GameAccessPath("chara.svo/TITLE.DAT/2/E_A101_TITLE", () =>
                {
                    Patcher.Action("Title Menu", () =>
                    {
                        Patcher.GameGetTXM("90", "91", (Txm) =>
                        {
                            //Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.E_A101_TITLE_P001, "E_A101_TITLE_P001", "E_A101_TITLE_P001_D", "E_A101_TITLE_P001_F");
                            Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.E_A101_TITLE_P001, "E_A101_TITLE_P001");
                            Patcher.UpdateTxm2DWithEmpty(Txm, "E_A101_TITLE_P001_D", "E_A101_TITLE_P001_F");
                            Patcher.ProgressHandler.IncrementLevelProgress();
                            
                            //Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.E_A101_TITLE_PUSH, "E_A101_TITLE_PUSH", "E_A101_TITLE_PUSH_D", "E_A101_TITLE_PUSH_F");
                            Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.E_A101_TITLE_PUSH, "E_A101_TITLE_PUSH");
                            Patcher.UpdateTxm2DWithEmpty(Txm, "E_A101_TITLE_PUSH_D", "E_A101_TITLE_PUSH_F");
                            Patcher.ProgressHandler.IncrementLevelProgress();

                            Patcher.UpdateTxm2DWithPng(Txm, PatchPaths.E_A101_TITLE_CREDIT_E, "E_A101_TITLE_CREDIT_E");
                            Patcher.ProgressHandler.IncrementLevelProgress();
                        });
                    });
                });
            });
		}
	}
}